# 09 — Ordenación, Filtrado y Paginación

> Basado en la documentación oficial de Microsoft (learn.microsoft.com)
> Adaptado al proyecto ClinicaMedica — modelo Paciente.

---

## ¿Por qué estos tres elementos juntos?

| Elemento | Función | Por qué es importante |
|---|---|---|
| `ViewData["XSortParm"]` | Pasa los parámetros de ordenación a los Tag Helpers de la vista | Toggle asc/desc al hacer clic en el encabezado |
| `currentFilter` | Preserva el texto de búsqueda al cambiar de página | Sin esto el filtro se pierde al paginar |
| `PaginatedList<T>` | Clase genérica reutilizable con Skip/Take y HasPreviousPage | Funciona con cualquier modelo |
| `asp-route-*` | Tag Helpers que construyen la URL con query string | Reemplazan la construcción manual de URLs |

---

## Paso 1 — Clase PaginatedList\<T\>

### ¿Por qué una clase separada?

Microsoft propone extraer la lógica de paginación a una clase genérica `PaginatedList<T>` reutilizable.
Hereda de `List<T>`, ejecuta el conteo y el Skip/Take de forma asíncrona,
y expone `HasPreviousPage` y `HasNextPage` para habilitar/deshabilitar los botones de navegación.

### Crear `PaginatedList.cs` en la raíz del proyecto (junto a Program.cs)

```csharp
// PaginatedList.cs
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedica;

public class PaginatedList<T> : List<T>
{
    public int PageIndex  { get; private set; }
    public int TotalPages { get; private set; }

    // Constructor privado — se llama solo desde CreateAsync
    public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex  = pageIndex;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        this.AddRange(items);  // PaginatedList ES una List<T>
    }

    // Propiedades para habilitar / deshabilitar botones de navegación
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage     => PageIndex < TotalPages;

    // Factory asíncrono — los constructores no pueden ser async
    public static async Task<PaginatedList<T>> CreateAsync(
        IQueryable<T> source, int pageIndex, int pageSize)
    {
        var count = await source.CountAsync();           // 1. Contar el total
        var items = await source
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();                              // 2. Traer solo la página
        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }
}
```

> 💡 Se usa un método factory estático `CreateAsync()` en vez de un constructor
> porque los constructores de C# no pueden ser `async`.
> Este patrón es el recomendado por Microsoft para paginación asíncrona con EF Core.

### Referencia de propiedades

| Propiedad / Método | Descripción |
|---|---|
| `PageIndex` | Número de la página actual |
| `TotalPages` | Total de páginas: `Math.Ceiling(count / pageSize)` |
| `HasPreviousPage` | `true` si `PageIndex > 1` — controla si el botón Anterior está habilitado |
| `HasNextPage` | `true` si `PageIndex < TotalPages` — controla el botón Siguiente |
| `CreateAsync()` | Ejecuta `CountAsync()` + Skip/Take en una sola operación async |

---

## Paso 2 — Ordenación con ViewData

### El mecanismo de toggle asc/desc

El patrón de Microsoft usa `ViewData` para pasar el valor del *siguiente* `sortOrder`
a cada encabezado de columna. La lógica es: si ya estás ordenando por esa columna
de forma ascendente, el link debe apuntar a la versión descendente, y viceversa.

```csharp
// Controllers/PacientesController.cs — solo la parte de ordenación

public async Task<IActionResult> Index(string sortOrder)
{
    // Lógica toggle para 'Nombre':
    // Si sortOrder es null/vacío (= ya en nombre ASC) → siguiente clic = DESC
    // En cualquier otro caso → siguiente clic = ASC (string vacío = default)
    ViewData["NombreSortParm"]   = string.IsNullOrEmpty(sortOrder) ? "nombre_desc" : "";
    ViewData["ApellidoSortParm"] = sortOrder == "Apellido"  ? "apellido_desc"  : "Apellido";
    ViewData["EmailSortParm"]    = sortOrder == "Email"     ? "email_desc"     : "Email";

    var pacientes = from p in _context.Pacientes select p;

    switch (sortOrder)
    {
        case "nombre_desc":   pacientes = pacientes.OrderByDescending(p => p.Nombre);   break;
        case "Apellido":      pacientes = pacientes.OrderBy(p => p.Apellido);            break;
        case "apellido_desc": pacientes = pacientes.OrderByDescending(p => p.Apellido); break;
        case "Email":         pacientes = pacientes.OrderBy(p => p.Email);               break;
        case "email_desc":    pacientes = pacientes.OrderByDescending(p => p.Email);    break;
        default:              pacientes = pacientes.OrderBy(p => p.Nombre);              break;
    }

    return View(await pacientes.AsNoTracking().ToListAsync());
}
```

### Tabla de estado del toggle por columna

| `sortOrder` actual | Orden activo | Link Nombre | Link Apellido | Link Email |
|---|---|---|---|---|
| (vacío / null) | Nombre ASC (default) | `nombre_desc` | `Apellido` | `Email` |
| `nombre_desc` | Nombre DESC | `""` (→ ASC) | `Apellido` | `Email` |
| `Apellido` | Apellido ASC | `nombre_desc` | `apellido_desc` | `Email` |
| `apellido_desc` | Apellido DESC | `nombre_desc` | `Apellido` | `Email` |

---

## Paso 3 — Filtro de búsqueda con currentFilter

### El problema: el filtro se pierde al paginar

Cuando el usuario filtra por "García" y luego hace clic en la página 2,
el formulario GET no reenvía el `searchString` — se pierde.

La solución de Microsoft es pasar `currentFilter` como parámetro del método `Index`
y propagarlo en todos los links de paginación y ordenación.

```csharp
// Controllers/PacientesController.cs — ordenación + filtrado con currentFilter

public async Task<IActionResult> Index(
    string sortOrder,
    string currentFilter,   // el filtro activo en este momento
    string searchString,    // el valor que acaba de escribir el usuario
    int? pageNumber)
{
    // ViewData para encabezados ordenables
    ViewData["CurrentSort"]      = sortOrder;  // para los links de paginación
    ViewData["NombreSortParm"]   = string.IsNullOrEmpty(sortOrder) ? "nombre_desc" : "";
    ViewData["ApellidoSortParm"] = sortOrder == "Apellido"  ? "apellido_desc"  : "Apellido";
    ViewData["EmailSortParm"]    = sortOrder == "Email"     ? "email_desc"     : "Email";

    // Lógica de currentFilter
    // Si el usuario escribió algo nuevo → volver a página 1
    if (searchString != null)
        pageNumber = 1;
    else
        searchString = currentFilter;  // al paginar: restaurar el filtro anterior

    ViewData["CurrentFilter"] = searchString;  // para repoblar el textbox en la vista

    var pacientes = from p in _context.Pacientes select p;

    if (!string.IsNullOrEmpty(searchString))
        pacientes = pacientes.Where(p =>
            p.Nombre.Contains(searchString) ||
            p.Apellido.Contains(searchString));

    switch (sortOrder)
    {
        case "nombre_desc":   pacientes = pacientes.OrderByDescending(p => p.Nombre);   break;
        case "Apellido":      pacientes = pacientes.OrderBy(p => p.Apellido);            break;
        case "apellido_desc": pacientes = pacientes.OrderByDescending(p => p.Apellido); break;
        case "Email":         pacientes = pacientes.OrderBy(p => p.Email);               break;
        case "email_desc":    pacientes = pacientes.OrderByDescending(p => p.Email);    break;
        default:              pacientes = pacientes.OrderBy(p => p.Nombre);              break;
    }

    int pageSize = 10;
    return View(await PaginatedList<Paciente>.CreateAsync(
        pacientes.AsNoTracking(), pageNumber ?? 1, pageSize));
}
```

> ⚠️ La distinción `searchString` / `currentFilter` es clave:
> `searchString` viene del textbox (puede ser null si el usuario solo cambió de página).
> `currentFilter` viene como parámetro de los links de paginación.
> Si `searchString != null` significa que el usuario filtró de nuevo → reset a página 1.

---

## Paso 4 — Controlador Index completo (versión final)

```csharp
// Controllers/PacientesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicaMedica.Models;

namespace ClinicaMedica.Controllers;

public class PacientesController : Controller
{
    private readonly ClinicaMedicaContext _context;

    public PacientesController(ClinicaMedicaContext context) => _context = context;

    public async Task<IActionResult> Index(
        string sortOrder,
        string currentFilter,
        string searchString,
        int? pageNumber)
    {
        // 1. ViewData para encabezados ordenables
        ViewData["CurrentSort"]      = sortOrder;
        ViewData["NombreSortParm"]   = string.IsNullOrEmpty(sortOrder) ? "nombre_desc" : "";
        ViewData["ApellidoSortParm"] = sortOrder == "Apellido"  ? "apellido_desc"  : "Apellido";
        ViewData["EmailSortParm"]    = sortOrder == "Email"     ? "email_desc"     : "Email";

        // 2. Gestión del filtro activo
        if (searchString != null)
            pageNumber = 1;
        else
            searchString = currentFilter;

        ViewData["CurrentFilter"] = searchString;

        // 3. Query base
        IQueryable<Paciente> pacientes = _context.Pacientes;

        // 4. Filtro por texto
        if (!string.IsNullOrEmpty(searchString))
            pacientes = pacientes.Where(p =>
                p.Nombre.Contains(searchString) ||
                p.Apellido.Contains(searchString));

        // 5. Ordenación con switch expression (.NET 8)
        pacientes = sortOrder switch
        {
            "nombre_desc"   => pacientes.OrderByDescending(p => p.Nombre),
            "Apellido"      => pacientes.OrderBy(p => p.Apellido),
            "apellido_desc" => pacientes.OrderByDescending(p => p.Apellido),
            "Email"         => pacientes.OrderBy(p => p.Email),
            "email_desc"    => pacientes.OrderByDescending(p => p.Email),
            _               => pacientes.OrderBy(p => p.Nombre),
        };

        // 6. Paginar con PaginatedList<T>
        int pageSize = 10;
        return View(await PaginatedList<Paciente>.CreateAsync(
            pacientes.AsNoTracking(), pageNumber ?? 1, pageSize));
    }

    // Los demás actions (Details, Create, Edit, Delete) no cambian
}
```

> 💡 En .NET 8+ se puede reemplazar el `switch/case` por una **switch expression** (con `=>`).
> Ambas formas son correctas; la switch expression es más compacta.

---

## Paso 5 — Vista Index completa con Tag Helpers

### Cambiar el @model a PaginatedList\<Paciente\>

La vista ahora recibe un `PaginatedList<Paciente>` (que hereda de `List<Paciente>`),
por lo que el `foreach` sigue funcionando igual.
Se agregan `HasPreviousPage` / `HasNextPage` para los botones.

```cshtml
@* Views/Pacientes/Index.cshtml *@
@model PaginatedList<ClinicaMedica.Models.Paciente>

@{
    ViewData["Title"] = "Pacientes";
}

<div class="d-flex justify-content-between align-items-center mb-3">
    <h2>Pacientes</h2>
    <a asp-action="Create" class="btn btn-primary">+ Nuevo Paciente</a>
</div>

@* Formulario de búsqueda — method=get → el filtro queda en la URL *@
<form asp-action="Index" method="get" class="mb-3">
    <div class="input-group" style="max-width:420px">
        <input type="text" name="SearchString"
               class="form-control"
               placeholder="Buscar por nombre o apellido..."
               value="@ViewData["CurrentFilter"]" />
        <button type="submit" class="btn btn-primary">🔍 Buscar</button>
        <a asp-action="Index" class="btn btn-outline-secondary">✕ Limpiar</a>
    </div>
</form>
```

### Tabla con encabezados ordenables (asp-route-*)

```cshtml
<table class="table table-bordered table-hover">
    <thead class="table-dark">
        <tr>
            @* Encabezado Nombre — lleva currentFilter para no perder el filtro al ordenar *@
            <th>
                <a asp-action="Index"
                   asp-route-sortOrder="@ViewData["NombreSortParm"]"
                   asp-route-currentFilter="@ViewData["CurrentFilter"]"
                   class="text-white text-decoration-none">
                    Nombre
                    @(ViewData["CurrentSort"]?.ToString() == ""           ? "▲" :
                      ViewData["CurrentSort"]?.ToString() == "nombre_desc"? "▼" : "⇅")
                </a>
            </th>
            <th>
                <a asp-action="Index"
                   asp-route-sortOrder="@ViewData["ApellidoSortParm"]"
                   asp-route-currentFilter="@ViewData["CurrentFilter"]"
                   class="text-white text-decoration-none">
                    Apellido
                    @(ViewData["CurrentSort"]?.ToString() == "Apellido"      ? "▲" :
                      ViewData["CurrentSort"]?.ToString() == "apellido_desc" ? "▼" : "⇅")
                </a>
            </th>
            <th>
                <a asp-action="Index"
                   asp-route-sortOrder="@ViewData["EmailSortParm"]"
                   asp-route-currentFilter="@ViewData["CurrentFilter"]"
                   class="text-white text-decoration-none">
                    Email
                    @(ViewData["CurrentSort"]?.ToString() == "Email"      ? "▲" :
                      ViewData["CurrentSort"]?.ToString() == "email_desc" ? "▼" : "⇅")
                </a>
            </th>
            <th>Teléfono</th>
            <th>Acciones</th>
        </tr>
    </thead>
    <tbody>
        @if (!Model.Any())
        {
            <tr><td colspan="5" class="text-center text-muted">Sin resultados.</td></tr>
        }
        @foreach (var paciente in Model)
        {
            <tr>
                <td>@paciente.Nombre</td>
                <td>@paciente.Apellido</td>
                <td>@paciente.Email</td>
                <td>@paciente.Telefono</td>
                <td>
                    <a asp-action="Edit"    asp-route-id="@paciente.Id" class="btn btn-warning btn-sm">Editar</a>
                    <a asp-action="Details" asp-route-id="@paciente.Id" class="btn btn-info btn-sm">Ver</a>
                    <a asp-action="Delete"  asp-route-id="@paciente.Id" class="btn btn-danger btn-sm">Eliminar</a>
                </td>
            </tr>
        }
    </tbody>
</table>
```

### Botones de paginación Anterior / Siguiente

```cshtml
@* Paginación — patrón oficial Microsoft *@
@{
    var prevDisabled = !Model.HasPreviousPage ? "disabled" : "";
    var nextDisabled = !Model.HasNextPage     ? "disabled" : "";
}

<div class="d-flex justify-content-between align-items-center mt-2">
    <small class="text-muted">
        Página @Model.PageIndex de @Model.TotalPages
    </small>

    <div class="d-flex gap-1">
        <a asp-action="Index"
           asp-route-sortOrder="@ViewData["CurrentSort"]"
           asp-route-pageNumber="@(Model.PageIndex - 1)"
           asp-route-currentFilter="@ViewData["CurrentFilter"]"
           class="btn btn-outline-secondary btn-sm @prevDisabled">
            &laquo; Anterior
        </a>

        @* Números de página — ventana ±2 alrededor de la página actual *@
        @for (int i = Math.Max(1, Model.PageIndex - 2);
                  i <= Math.Min(Model.TotalPages, Model.PageIndex + 2); i++)
        {
            <a asp-action="Index"
               asp-route-sortOrder="@ViewData["CurrentSort"]"
               asp-route-pageNumber="@i"
               asp-route-currentFilter="@ViewData["CurrentFilter"]"
               class="btn btn-sm @(i == Model.PageIndex ? "btn-primary" : "btn-outline-secondary")">
                @i
            </a>
        }

        <a asp-action="Index"
           asp-route-sortOrder="@ViewData["CurrentSort"]"
           asp-route-pageNumber="@(Model.PageIndex + 1)"
           asp-route-currentFilter="@ViewData["CurrentFilter"]"
           class="btn btn-outline-secondary btn-sm @nextDisabled">
            Siguiente &raquo;
        </a>
    </div>
</div>
```

> 💡 Los Tag Helpers `asp-route-*` generan automáticamente los query strings:
> `asp-route-pageNumber` genera `&pageNumber=2`,
> `asp-route-currentFilter` genera `&currentFilter=Garcia`, etc.
> No es necesario construir las URLs manualmente.

---

## Flujo completo: cómo se preservan filtros y ordenación

| Acción del usuario | Parámetros enviados | Resultado |
|---|---|---|
| Entra a /Pacientes | todos null | Nombre ASC, sin filtro, página 1 |
| Escribe "García" y busca | `searchString="García"`, pageNumber=null → reset 1 | Filtra por "García", página 1 |
| Clic en encabezado "Apellido" | `sortOrder="Apellido"`, `currentFilter="García"` | Ordena por Apellido ASC, conserva filtro |
| Clic en paginación → página 2 | `pageNumber=2`, `currentFilter="García"`, `sortOrder="Apellido"` | Página 2, mantiene filtro y orden |
| Clic en "Apellido" de nuevo | `sortOrder="apellido_desc"`, `currentFilter="García"` | Invierte a Apellido DESC, mantiene filtro |
| Clic en "✕ Limpiar" | navega a /Pacientes sin params | Reset completo |

---

## Resumen del patrón

| Elemento | Descripción |
|---|---|
| `PaginatedList<T>` | Clase genérica en la raíz del proyecto — `CreateAsync(IQueryable, page, size)` |
| Ordenación | `ViewData["XSortParm"]` con toggle ternario por cada columna |
| Filtro | `searchString` + `currentFilter` — reset a página 1 si hay nueva búsqueda |
| Switch ordenación | `switch(sortOrder)` con un case por columna y dirección |
| Vista — `@model` | `PaginatedList<ClinicaMedica.Models.Paciente>` |
| Encabezados | `asp-route-sortOrder` + `asp-route-currentFilter` en cada `<th>` |
| Paginación | `asp-route-pageNumber` + `asp-route-currentFilter` + `disabled` con `@prevDisabled` |

📖 Tutorial oficial: [learn.microsoft.com/es-es/aspnet/core/data/ef-mvc/sort-filter-page](https://learn.microsoft.com/es-es/aspnet/core/data/ef-mvc/sort-filter-page)

---

[← Guía 08](./08-identity-login-roles.md) | [← Volver al README](../README.md)
