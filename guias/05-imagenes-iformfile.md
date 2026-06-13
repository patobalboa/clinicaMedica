# 05 — Subir imágenes con IFormFile

---

## ¿Cómo funciona la subida de archivos?

### La analogía del correo postal

Cuando subes una imagen desde el formulario, llega como un "sobre" al controlador.
Ese sobre es el `IFormFile`. Tu trabajo es:
1. Abrir el sobre
2. Darle un nombre único al archivo (para que no se pisen entre sí)
3. Guardarlo en la carpeta correcta del servidor
4. Guardar **solo la ruta** en la base de datos

> **¿Por qué guardar la ruta y no el archivo en la BD?**
> La base de datos no está diseñada para guardar archivos grandes.
> Se guarda `/uploads/imagen123.jpg` y el archivo real vive en `wwwroot/uploads/`.

---

## Paso 1 — El formulario necesita enctype

Para que el formulario pueda enviar archivos, necesita el atributo `enctype`:

```html
<!-- Views/Pacientes/Create.cshtml -->
<!-- SIN enctype los archivos no llegan al controlador -->
<form asp-action="Create" method="post" enctype="multipart/form-data">

    <!-- campos normales del paciente -->
    <div class="mb-3">
        <label asp-for="Nombre" class="form-label"></label>
        <input asp-for="Nombre" class="form-control" />
        <span asp-validation-for="Nombre" class="text-danger"></span>
    </div>

    <!-- campo para la foto -->
    <div class="mb-3">
        <label class="form-label">Foto del paciente</label>
        <input type="file" name="fotoArchivo" class="form-control"
               accept=".jpg,.jpeg,.png" />
    </div>

    <button type="submit" class="btn btn-primary">Guardar</button>
</form>
```

---

## Paso 2 — El controlador recibe y guarda la imagen

```csharp
// Controllers/PacientesController.cs — acción Create POST

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(
    Paciente paciente,
    IFormFile? fotoArchivo)   // recibe el archivo — el nombre debe coincidir con name="fotoArchivo"
{
    if (fotoArchivo != null && fotoArchivo.Length > 0)
    {
        // 1. Crear un nombre único para evitar que dos fotos se llamen igual
        string extension = Path.GetExtension(fotoArchivo.FileName);    // ej: ".jpg"
        string nombreUnico = Guid.NewGuid().ToString() + extension;     // ej: "a3f2b1c4...jpg"

        // 2. Armar la ruta física dentro de wwwroot/uploads
        string carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        Directory.CreateDirectory(carpeta);  // crear la carpeta si no existe

        string rutaFisica = Path.Combine(carpeta, nombreUnico);

        // 3. Copiar el archivo al servidor
        using var flujo = new FileStream(rutaFisica, FileMode.Create);
        await fotoArchivo.CopyToAsync(flujo);

        // 4. Guardar SOLO la ruta relativa en la base de datos
        paciente.Foto = "/uploads/" + nombreUnico;
    }

    if (ModelState.IsValid)
    {
        _context.Add(paciente);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    return View(paciente);
}
```

---

## Paso 3 — Mostrar la imagen en las vistas

En `Index.cshtml` o `Details.cshtml`:

```html
<!-- Mostrar la foto si existe, o un texto si no tiene -->
@if (!string.IsNullOrEmpty(item.Foto))
{
    <img src="@item.Foto" alt="Foto de @item.Nombre"
         style="width: 60px; height: 60px; object-fit: cover;"
         class="rounded-circle" />
}
else
{
    <span class="text-muted">Sin foto</span>
}
```

---

## Paso 4 — Editar también debe manejar la imagen

En el Edit, si el usuario no sube una nueva foto, hay que conservar la foto anterior:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(
    int id,
    Paciente paciente,
    IFormFile? fotoArchivo)
{
    if (id != paciente.Id) return NotFound();

    if (fotoArchivo != null && fotoArchivo.Length > 0)
    {
        // Subió foto nueva → guardarla igual que en Create
        string extension = Path.GetExtension(fotoArchivo.FileName);
        string nombreUnico = Guid.NewGuid().ToString() + extension;
        string carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        Directory.CreateDirectory(carpeta);
        string rutaFisica = Path.Combine(carpeta, nombreUnico);

        using var flujo = new FileStream(rutaFisica, FileMode.Create);
        await fotoArchivo.CopyToAsync(flujo);

        paciente.Foto = "/uploads/" + nombreUnico;
    }
    // Si fotoArchivo es null → no tocar paciente.Foto
    // (el formulario debe incluir un hidden con la foto actual)

    if (ModelState.IsValid)
    {
        _context.Update(paciente);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    return View(paciente);
}
```

Agrega este hidden en el formulario de Edit para preservar la foto actual:

```html
<!-- En Views/Pacientes/Edit.cshtml — dentro del form -->
<input type="hidden" asp-for="Foto" />

<!-- Mostrar la foto actual si tiene -->
@if (!string.IsNullOrEmpty(Model.Foto))
{
    <div class="mb-3">
        <label class="form-label">Foto actual</label><br/>
        <img src="@Model.Foto" style="max-width: 150px;" class="img-thumbnail" />
    </div>
}
<div class="mb-3">
    <label class="form-label">Cambiar foto (opcional)</label>
    <input type="file" name="fotoArchivo" class="form-control" accept=".jpg,.jpeg,.png" />
</div>
```

---

## Errores comunes

| Error | Causa | Solución |
|---|---|---|
| La imagen no llega al controlador | Falta `enctype="multipart/form-data"` | Agregarlo al `<form>` |
| El nombre en el form no coincide | `name="foto"` pero el parámetro es `IFormFile fotoArchivo` | Deben ser iguales |
| La imagen no se muestra | La ruta guardada en BD no coincide con donde se guardó | Verificar que sea `/uploads/nombrearchivo.ext` |
| La carpeta no existe | No se creó `wwwroot/uploads/` | El código tiene `Directory.CreateDirectory(carpeta)` para eso |

---

[← Guía 04](./04-validaciones-formularios.md) | [Siguiente: Paginación →](./06-paginacion.md)
