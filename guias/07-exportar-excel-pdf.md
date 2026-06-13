# 07 — Exportar a Excel y PDF

---

## Instalación

```powershell
Install-Package ClosedXML
Install-Package QuestPDF
```

Agregar en `Program.cs` (una sola vez, antes de `builder.Build()`):

```csharp
// Licencia gratuita para uso académico
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
```

---

## Exportar a Excel con ClosedXML

### La analogía

ClosedXML es como un robot que llena una planilla Excel por ti.
Tú le dices "escribe esto en la celda A1", y él construye el archivo.

```csharp
// Controllers/PacientesController.cs

using ClosedXML.Excel;

public async Task<IActionResult> ExportarExcel()
{
    var pacientes = await _context.Pacientes
        .AsNoTracking()
        .OrderBy(p => p.Apellido)
        .ToListAsync();

    using var libro = new XLWorkbook();
    var hoja = libro.Worksheets.Add("Pacientes");

    // Fila 1: encabezados en negrita
    hoja.Cell(1, 1).Value = "Nombre";
    hoja.Cell(1, 2).Value = "Apellido";
    hoja.Cell(1, 3).Value = "Teléfono";
    hoja.Cell(1, 4).Value = "Email";

    // Estilo para los encabezados (opcional pero queda mejor)
    var rango = hoja.Range(1, 1, 1, 4);
    rango.Style.Font.Bold = true;
    rango.Style.Fill.BackgroundColor = XLColor.SteelBlue;
    rango.Style.Font.FontColor = XLColor.White;

    // Filas de datos — empieza en la fila 2
    for (int i = 0; i < pacientes.Count; i++)
    {
        int fila = i + 2;
        hoja.Cell(fila, 1).Value = pacientes[i].Nombre;
        hoja.Cell(fila, 2).Value = pacientes[i].Apellido;
        hoja.Cell(fila, 3).Value = pacientes[i].Telefono ?? "";
        hoja.Cell(fila, 4).Value = pacientes[i].Email ?? "";
    }

    // Ajustar ancho de columnas automáticamente
    hoja.Columns().AdjustToContents();

    // Guardar en memoria y enviar al navegador
    using var flujo = new MemoryStream();
    libro.SaveAs(flujo);

    return File(
        flujo.ToArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "pacientes.xlsx");
}
```

### El botón en Index.cshtml

```html
<a asp-action="ExportarExcel" class="btn btn-success">
    📥 Exportar Excel
</a>
```

---

## Exportar a PDF con QuestPDF

### La analogía

QuestPDF es como un diseñador que toma los datos y los maqueta
en un documento PDF con título, tabla y números de página.

```csharp
// Controllers/PacientesController.cs

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public async Task<IActionResult> ExportarPdf()
{
    var pacientes = await _context.Pacientes
        .AsNoTracking()
        .OrderBy(p => p.Apellido)
        .ToListAsync();

    byte[] pdf = Document.Create(documento =>
    {
        documento.Page(pagina =>
        {
            pagina.Margin(30);

            // Título
            pagina.Header()
                .Text($"Listado de Pacientes — {DateTime.Today:dd/MM/yyyy}")
                .FontSize(16)
                .Bold()
                .AlignCenter();

            // Tabla con los datos
            pagina.Content()
                .PaddingTop(20)
                .Table(tabla =>
                {
                    // Definir el ancho de cada columna
                    tabla.ColumnsDefinition(col =>
                    {
                        col.RelativeColumn(2); // Nombre
                        col.RelativeColumn(2); // Apellido
                        col.RelativeColumn(2); // Teléfono
                        col.RelativeColumn(3); // Email
                    });

                    // Encabezados
                    tabla.Header(encabezado =>
                    {
                        encabezado.Cell().Background("#2E75B6").Padding(5)
                            .Text("Nombre").FontColor("#FFFFFF").Bold();
                        encabezado.Cell().Background("#2E75B6").Padding(5)
                            .Text("Apellido").FontColor("#FFFFFF").Bold();
                        encabezado.Cell().Background("#2E75B6").Padding(5)
                            .Text("Teléfono").FontColor("#FFFFFF").Bold();
                        encabezado.Cell().Background("#2E75B6").Padding(5)
                            .Text("Email").FontColor("#FFFFFF").Bold();
                    });

                    // Filas de datos
                    foreach (var p in pacientes)
                    {
                        tabla.Cell().Padding(5).Text(p.Nombre);
                        tabla.Cell().Padding(5).Text(p.Apellido);
                        tabla.Cell().Padding(5).Text(p.Telefono ?? "—");
                        tabla.Cell().Padding(5).Text(p.Email ?? "—");
                    }
                });

            // Pie de página con número de página
            pagina.Footer()
                .AlignRight()
                .Text(texto =>
                {
                    texto.Span("Página ");
                    texto.CurrentPageNumber();
                    texto.Span(" de ");
                    texto.TotalPages();
                });
        });
    }).GeneratePdf();

    return File(pdf, "application/pdf", "pacientes.pdf");
}
```

### El botón en Index.cshtml

```html
<a asp-action="ExportarPdf" class="btn btn-danger">
    📄 Exportar PDF
</a>
```

---

## Repite para cada modelo

El patrón es siempre el mismo. Solo cambian:
- El nombre del modelo y sus propiedades
- El nombre del archivo descargado (`pacientes.xlsx`, `medicos.xlsx`, etc.)
- Las columnas de la tabla

---

[← Guía 06](./06-paginacion.md) | [Siguiente: Login y Roles →](./08-identity-login-roles.md)
