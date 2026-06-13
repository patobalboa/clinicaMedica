# 09 — Filtros, ordenación y paginación combinados

> Esta guía es para la Evaluación 3.
> Extiende la paginación de la guía 06.

---

## ¿Qué problema resuelve esto?

Con paginación simple, si el usuario filtra "González" y luego pasa a la página 2,
el filtro se pierde. Esta guía muestra cómo conservar filtro + orden + página juntos.

---

## El controlador completo

```csharp
// Controllers/PacientesController.cs

using X.PagedList;
using X.PagedList.Extensions;

public async Task<IActionResult> Index(
    string? sortOrder,       // columna y dirección de ordenación
    string? currentFilter,   // filtro guardado (para cuando el usuario cambia de página)
    string? searchString,    // lo que acaba de escribir el usuario
    int? pagina)             // página actual
{
    // ── 1. Pasar los parámetros de ordenación a la vista ──────────────────
    // La vista los usa para construir los links de los encabezados de columna
    ViewData["OrdenActual"]    = sortOrder;
    ViewData["OrdenNombre"]    = string.IsNullOrEmpty(sortOrder) ? "nombre_desc" : "";
    ViewData["OrdenApellido"]  = sortOrder == "Apellido" ? "apellido_desc" : "Apellido";
    ViewData["OrdenEmail"]     = sortOrder == "Email"    ? "email_desc"    : "Email";

    // ── 2. Gestionar el filtro ────────────────────────────────────────────
    // Si el usuario escribió algo nuevo, volver a la página 1
    if (searchString != null)
        pagina = 1;
    else
        searchString = currentFilter;  // si solo cambió de página, restaurar el filtro

    ViewData["FiltroActual"] = searchString;  // para repoblar el textbox en la vista

    // ── 3. Consulta base ──────────────────────────────────────────────────
    var pacientes = _context.Pacientes.AsNoTracking().AsQueryable();

    // ── 4. Aplicar filtro si hay texto de búsqueda ────────────────────────
    if (!string.IsNullOrEmpty(searchString))
        pacientes = pacientes.Where(p =>
            p.Nombre.Contains(searchString) ||
            p.Apellido.Contains(searchString));

    // ── 5. Aplicar ordenación ─────────────────────────────────────────────
    pacientes = sortOrder switch
    {
        "nombre_desc"   => pacientes.OrderByDescending(p => p.Nombre),
        "Apellido"      => pacientes.OrderBy(p => p.Apellido),
        "apellido_desc" => pacientes.OrderByDescending(p => p.Apellido),
        "Email"         => pacientes.OrderBy(p => p.Email),
        "email_desc"    => pacientes.OrderByDescending(p => p.Email),
        _               => pacientes.OrderBy(p => p.Nombre),  // default
    };

    // ── 6. Paginar ────────────────────────────────────────────────────────
    int registrosPorPagina = 10;
    var resultado = pacientes.ToPagedList(pagina ?? 1, registrosPorPagina);

    return View(resultado);
}
```

---

## La vista con filtro + ordenación + paginación

```cshtml
@* Views/Pacientes/Index.cshtml *@
@using X.PagedList
@using X.PagedList.Mvc.Core
@model IPagedList<ClinicaMedica.Models.Paciente>

@{
    ViewData["Title"] = "Pacientes";
    var filtroActual = ViewData["FiltroActual"]?.ToString();
    var ordenActual  = ViewData["OrdenActual"]?.ToString();
}

<div class="d-flex justify-content-between align-items-center mb-3">
    <h2>Pacientes</h2>
    @if (User.IsInRole("Administrador") || User.IsInRole("Supervisor"))
    {
        <a asp-action="Create" class="btn btn-primary">+ Nuevo Paciente</a>
    }
</div>

@* Formulario de búsqueda — usa GET para que el filtro quede en la URL *@
<form asp-action="Index" method="get" class="mb-3">
    <div class="input-group" style="max-width:420px">
        <input type="text" name="searchString"
               class="form-control"
               placeholder="Buscar por nombre o apellido..."
               value="@filtroActual" />
        <button type="submit" class="btn btn-primary">🔍 Buscar</button>
        <a asp-action="Index" class="btn btn-outline-secondary">✕ Limpiar</a>
    </div>
</form>

@* Texto informativo *@
<p class="text-muted">
    Mostrando @Model.FirstItemOnPage–@Model.LastItemOnPage de @Model.TotalItemCount resultados
</p>

<table class="table table-bordered table-hover">
    <thead class="table-dark">
        <tr>
            @* Encabezados clicables para ordenar *@
            @* asp-route-sortOrder: el orden que se aplicará al hacer clic *@
            @* asp-route-currentFilter: conserva el filtro al cambiar el orden *@
            <th>
                <a asp-action="Index"
                   asp-route-sortOrder="@ViewData["OrdenNombre"]"
                   asp-route-currentFilter="@filtroActual"
                   class="text-white text-decoration-none">
                    Nombre
                    @(ordenActual == ""           ? "▲" :
                      ordenActual == "nombre_desc"? "▼" : "⇅")
                </a>
            </th>
            <th>
                <a asp-action="Index"
                   asp-route-sortOrder="@ViewData["OrdenApellido"]"
                   asp-route-currentFilter="@filtroActual"
                   class="text-white text-decoration-none">
                    Apellido
                    @(ordenActual == "Apellido"      ? "▲" :
                      ordenActual == "apellido_desc" ? "▼" : "⇅")
                </a>
            </th>
            <th>
                <a asp-action="Index"
                   asp-route-sortOrder="@ViewData["OrdenEmail"]"
                   asp-route-currentFilter="@filtroActual"
                   class="text-white text-decoration-none">
                    Email
                    @(ordenActual == "Email"      ? "▲" :
                      ordenActual == "email_desc" ? "▼" : "⇅")
                </a>
            </th>
            <th>Teléfono</th>
            <th>Acciones</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var paciente in Model)
        {
            <tr>
                <td>@paciente.Nombre</td>
                <td>@paciente.Apellido</td>
                <td>@paciente.Email</td>
                <td>@paciente.Telefono</td>
                <td>
                    <a asp-action="Details" asp-route-id="@paciente.Id" class="btn btn-info btn-sm">Ver</a>
                    @if (User.IsInRole("Administrador") || User.IsInRole("Supervisor"))
                    {
                        <a asp-action="Edit" asp-route-id="@paciente.Id" class="btn btn-warning btn-sm">Editar</a>
                    }
                    @if (User.IsInRole("Administrador"))
                    {
                        <a asp-action="Delete" asp-route-id="@paciente.Id" class="btn btn-danger btn-sm">Eliminar</a>
                    }
                </td>
            </tr>
        }
    </tbody>
</table>

@* Paginación — pasa el filtro y el orden para conservarlos al cambiar de página *@
@Html.PagedListPager(
    Model,
    pagina => Url.Action("Index", new {
        pagina,
        sortOrder    = ordenActual,
        currentFilter = filtroActual
    }),
    new X.PagedList.Mvc.Common.PagedListRenderOptions
    {
        LiElementClasses          = new[] { "page-item" },
        PageClasses               = new[] { "page-link" },
        UlElementClasses          = new[] { "pagination", "justify-content-center", "mt-3" },
        DisplayLinkToPreviousPage = PagedListDisplayMode.Always,
        DisplayLinkToNextPage     = PagedListDisplayMode.Always,
        DisplayLinkToFirstPage    = PagedListDisplayMode.Never,
        DisplayLinkToLastPage     = PagedListDisplayMode.Never,
    }
)
```

---

## ¿Cómo se conectan las tres piezas?

| Acción del usuario | Qué pasa |
|---|---|
| Entra a /Pacientes | Sin parámetros → orden default, sin filtro, página 1 |
| Escribe "González" y busca | `searchString="González"` → `pagina=1` (reset) |
| Hace clic en "Apellido" | `sortOrder="Apellido"`, `currentFilter="González"` (se conserva el filtro) |
| Pasa a la página 2 | `pagina=2`, `currentFilter="González"`, `sortOrder="Apellido"` (todo se conserva) |
| Clic en "✕ Limpiar" | Navega a /Pacientes sin parámetros → reset completo |

---

## Repite para cada modelo

El patrón es idéntico para Medicos, Especialidads, Cita y Tratamientos.
Solo cambian los nombres de las columnas y propiedades.

---

[← Guía 08](./08-identity-login-roles.md) | [← Volver al README](../README.md)
