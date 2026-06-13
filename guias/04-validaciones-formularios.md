# 04 — Validaciones y formularios

---

## ¿Para qué sirven las validaciones?

Imagina un formulario de papel. Antes de que llegue a la oficina,
alguien revisa que todos los campos estén llenos y con el formato correcto.
Las validaciones hacen exactamente eso: verifican los datos **antes de guardarlos**.

Hay dos tipos:
- **Servidor:** verifica en el controlador antes de guardar en la BD
- **Cliente:** verifica en el navegador antes de enviar el formulario (más rápido para el usuario)

---

## DataAnnotations — reglas en el modelo

Las DataAnnotations son atributos que se ponen sobre las propiedades del modelo.
Como el scaffolding genera modelos `partial`, agrega las anotaciones en un archivo separado
para que no se pierdan si vuelves a hacer scaffolding.

Crea `Models/PacienteMetadata.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace ClinicaMedica.Models;

// Clase con las anotaciones — separada del modelo generado
[MetadataType(typeof(PacienteMetadata))]
public partial class Paciente { }

public class PacienteMetadata
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 2,
        ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Apellido")]
    public string Apellido { get; set; } = null!;

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de nacimiento")]
    public DateOnly? FechaNacimiento { get; set; }

    [Phone(ErrorMessage = "Ingrese un teléfono válido.")]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
    [Display(Name = "Correo electrónico")]
    public string? Email { get; set; }
}
```

### Anotaciones más usadas

| Anotación | Qué valida |
|---|---|
| `[Required]` | Campo obligatorio |
| `[StringLength(100, MinimumLength=2)]` | Longitud máxima y mínima |
| `[EmailAddress]` | Formato de email |
| `[Phone]` | Formato de teléfono |
| `[DataType(DataType.Date)]` | Muestra el campo como selector de fecha |
| `[Display(Name = "...")]` | El nombre que aparece en el formulario |
| `[Range(1, 999)]` | Rango numérico |

---

## ModelState.IsValid — el guardián

En el controlador, `ModelState.IsValid` revisa que todas las reglas se cumplan
**antes de guardar en la base de datos**:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Paciente paciente)
{
    if (ModelState.IsValid)
    {
        // ✅ Todas las validaciones pasaron → guardar
        _context.Add(paciente);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // ❌ Alguna validación falló → devolver el formulario con los errores
    return View(paciente);
}
```

> **Regla importante:** nunca guardes en la BD sin verificar `ModelState.IsValid` primero.

---

## La vista del formulario

El scaffolding genera los formularios automáticamente, pero es útil entender qué hace cada parte:

```html
<!-- Views/Pacientes/Create.cshtml -->
@model ClinicaMedica.Models.Paciente

<form asp-action="Create" method="post">

    <!-- asp-validation-summary muestra todos los errores juntos -->
    <div asp-validation-summary="ModelOnly" class="text-danger"></div>

    <div class="mb-3">
        <!-- asp-for genera el label con el nombre del Display -->
        <label asp-for="Nombre" class="form-label"></label>

        <!-- asp-for genera el input con el name, id y type correctos -->
        <input asp-for="Nombre" class="form-control" />

        <!-- asp-validation-for muestra el error de ese campo específico -->
        <span asp-validation-for="Nombre" class="text-danger"></span>
    </div>

    <div class="mb-3">
        <label asp-for="Apellido" class="form-label"></label>
        <input asp-for="Apellido" class="form-control" />
        <span asp-validation-for="Apellido" class="text-danger"></span>
    </div>

    <div class="mb-3">
        <label asp-for="FechaNacimiento" class="form-label"></label>
        <input asp-for="FechaNacimiento" class="form-control" />
        <span asp-validation-for="FechaNacimiento" class="text-danger"></span>
    </div>

    <button type="submit" class="btn btn-primary">Guardar</button>
    <a asp-action="Index" class="btn btn-secondary">Cancelar</a>

</form>

<!-- Validación del cliente (javascript) — va al final de la vista -->
@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

> **¿Para qué sirve `@section Scripts`?**
> El `_ValidationScriptsPartial` activa la validación en el navegador (javascript).
> Sin esto, el formulario solo valida cuando llega al servidor.
> Con esto, valida inmediatamente mientras el usuario escribe.

---

## Resultado esperado

Al terminar, al enviar un formulario vacío deberías ver:
- Mensajes de error en rojo junto a cada campo
- El formulario no se envía hasta que todo esté correcto
- Si hay un error en el servidor, el formulario vuelve con los mensajes

---

[← Guía 03](./03-modelos-scaffolding.md) | [Siguiente: Subir imágenes →](./05-imagenes-iformfile.md)
