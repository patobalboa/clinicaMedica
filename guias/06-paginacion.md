# 06 — Paginación con X.PagedList.Mvc.Core

---

## ¿Por qué paginar?

Si tienes 500 pacientes y los cargas todos en la tabla,
la página tarda mucho en cargar y es difícil de usar.
La paginación muestra solo 10 por vez, como los capítulos de un libro.

---

## Instalación

En la Consola del Administrador de paquetes:

```powershell
Install-Package X.PagedList.Mvc.Core
```

---

## El controlador

```csharp
// Controllers/PacientesController.cs

using X.PagedList;
using X.PagedList.Extensions;

public async Task<IActionResult> Index(int? pagina)
{
    int numeroPagina = pagina ?? 1;       // si no viene pagina, mostrar la 1
    int registrosPorPagina = 10;

    var pacientes = _context.Pacientes
        .AsNoTracking()
        .OrderBy(p => p.Apellido);        // ordenar antes de paginar

    // ToPagedList aplica Skip y Take internamente
    var resultado = pacientes.ToPagedList(numeroPagina, registrosPorPagina);

    return View(resultado);
}
```

> **¿Qué hace `AsNoTracking()`?**
> Le dice a EF Core que no necesita rastrear cambios en estos objetos.
> Como solo estamos mostrando datos (no editando), es más rápido.

---

## La vista

```cshtml
@* Views/Pacientes/Index.cshtml *@
@using X.PagedList
@using X.PagedList.Mvc.Core
@model IPagedList<ClinicaMedica.Models.Paciente>

@{
    ViewData["Title"] = "Pacientes";
}

<div class="d-flex justify-content-between align-items-center mb-3">
    <h2>Pacientes</h2>
    <a asp-action="Create" class="btn btn-primary">+ Nuevo Paciente</a>
</div>

<table class="table table-bordered table-hover">
    <thead class="table-dark">
        <tr>
            <th>Foto</th>
            <th>Nombre</th>
            <th>Apellido</th>
            <th>Teléfono</th>
            <th>Email</th>
            <th>Acciones</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var paciente in Model)
        {
            <tr>
                <td>
                    @if (!string.IsNullOrEmpty(paciente.Foto))
                    {
                        <img src="@paciente.Foto" style="width:50px;height:50px;object-fit:cover;" class="rounded-circle" />
                    }
                    else { <span class="text-muted">—</span> }
                </td>
                <td>@paciente.Nombre</td>
                <td>@paciente.Apellido</td>
                <td>@paciente.Telefono</td>
                <td>@paciente.Email</td>
                <td>
                    <a asp-action="Details" asp-route-id="@paciente.Id" class="btn btn-info btn-sm">Ver</a>
                    <a asp-action="Edit"    asp-route-id="@paciente.Id" class="btn btn-warning btn-sm">Editar</a>
                    <a asp-action="Delete"  asp-route-id="@paciente.Id" class="btn btn-danger btn-sm">Eliminar</a>
                </td>
            </tr>
        }
    </tbody>
</table>

@* Texto informativo — "Mostrando 1-10 de 47 resultados" *@
<p class="text-muted">
    Mostrando @Model.FirstItemOnPage–@Model.LastItemOnPage de @Model.TotalItemCount pacientes
</p>

@* Controles de paginación con Bootstrap *@
@Html.PagedListPager(
    Model,
    pagina => Url.Action("Index", new { pagina }),
    new X.PagedList.Mvc.Common.PagedListRenderOptions
    {
        LiElementClasses      = new[] { "page-item" },
        PageClasses           = new[] { "page-link" },
        UlElementClasses      = new[] { "pagination", "justify-content-center", "mt-3" },
        DisplayLinkToFirstPage  = PagedListDisplayMode.Never,
        DisplayLinkToLastPage   = PagedListDisplayMode.Never,
        DisplayLinkToPreviousPage = PagedListDisplayMode.Always,
        DisplayLinkToNextPage   = PagedListDisplayMode.Always,
    }
)
```

---

## Agregar el CSS de paginación

En `Views/Shared/_Layout.cshtml`, dentro del `<head>`:

```html
<link rel="stylesheet" href="~/lib/x-pagedlist/X.PagedList.Mvc.Core.css" />
```

> Si el archivo CSS no aparece en `wwwroot/lib/`, la configuración de Bootstrap
> en el `LiElementClasses` y `PageClasses` ya aplica los estilos correctos.

---

## Repite para cada modelo

Debes hacer lo mismo para: Medicos, Especialidads, Cita, Tratamientos.

El patrón es siempre el mismo:
1. Agregar `int? pagina` al método Index
2. Cambiar el `ToListAsync()` por `ToPagedList(numeroPagina, registrosPorPagina)`
3. Cambiar el `@model` de la vista a `IPagedList<TuModelo>`
4. Agregar el `@Html.PagedListPager(...)` al final de la vista

---

[← Guía 05](./05-imagenes-iformfile.md) | [Siguiente: Excel y PDF →](./07-exportar-excel-pdf.md)
