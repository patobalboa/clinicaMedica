# 01 — Cómo funciona MVC, async/await y Tag Helpers

> Antes de escribir código, entiende qué hace cada parte.
> Si entiendes esto, todo lo demás tiene sentido.

---

## ¿Qué es MVC?

MVC significa **Modelo - Vista - Controlador**. Es una forma de organizar el código
para que cada parte tenga una sola responsabilidad.

### La analogía del restaurante

Imagina que tu aplicación web es un restaurante:

| Parte de MVC | En el restaurante | En el código |
|---|---|---|
| **Modelo** | La cocina y los ingredientes | Las clases C# que representan los datos (Paciente, Medico, Cita) |
| **Vista** | El plato que llega a la mesa | El HTML que el usuario ve en el navegador (.cshtml) |
| **Controlador** | El mesero | Recibe el pedido (petición web), va a la cocina (modelo), trae el plato (vista) |

### ¿Qué pasa cuando el usuario abre /Pacientes?

```
1. El navegador envía una petición a /Pacientes
2. ASP.NET busca el PacientesController, acción Index
3. El controlador consulta la base de datos
4. Pasa los datos a la vista Index.cshtml
5. La vista genera el HTML con esos datos
6. El HTML llega al navegador del usuario
```

> **Regla de oro:** el Controlador nunca muestra HTML por sí solo,
> y la Vista nunca consulta la base de datos directamente.
> El Controlador es el intermediario.

---

## ¿Qué es async / await?

### La analogía del cajero de banco

Imagina que eres el único cajero de un banco.

**Sin async:** Cuando alguien paga un cheque, te quedas parado esperando
que el sistema bancario responda (3 segundos). Mientras esperas, nadie más puede ser atendido.

**Con async:** Cuando alguien paga un cheque, le dices al sistema
"avísame cuando termines" y mientras tanto atiendes a la siguiente persona.

En una aplicación web, el servidor atiende muchas personas al mismo tiempo.
Cuando consultas la base de datos, `async/await` libera el servidor
para atender otras peticiones mientras espera la respuesta.

### En el código

```csharp
// ❌ SIN async — el servidor queda bloqueado esperando la BD
public IActionResult Index()
{
    var pacientes = _context.Pacientes.ToList();  // bloquea
    return View(pacientes);
}

// ✅ CON async — el servidor queda libre mientras espera
public async Task<IActionResult> Index()
{
    var pacientes = await _context.Pacientes.ToListAsync();  // libera
    return View(pacientes);
}
```

### Las 3 reglas de async

| Regla | Ejemplo |
|---|---|
| Si el método es `async`, devuelve `Task<T>` | `async Task<IActionResult>` |
| Toda operación de BD lleva `await` | `await _context.SaveChangesAsync()` |
| Usa siempre la versión `Async` de los métodos | `ToListAsync()`, `FindAsync()`, `SaveChangesAsync()` |

---

## ¿Qué son los Tag Helpers?

### La analogía de los atajos de teclado

Cuando escribes `Ctrl+C`, no tienes que escribir el código para copiar —
el sistema lo hace por ti. Los Tag Helpers funcionan igual: son atajos
que generan HTML automáticamente y correctamente.

Son atributos que empiezan con `asp-` y se usan en los archivos `.cshtml`.

### Los más importantes

```html
<!-- asp-controller y asp-action: generan la URL correcta automáticamente -->
<a asp-controller="Pacientes" asp-action="Index">Ver pacientes</a>
<!-- Genera: <a href="/Pacientes">Ver pacientes</a> -->

<!-- asp-for: conecta el campo del formulario con la propiedad del modelo -->
<!-- Genera el id, name, type y el mensaje de error automáticamente -->
<input asp-for="Nombre" class="form-control" />
<span asp-validation-for="Nombre" class="text-danger"></span>

<!-- asp-route-id: agrega un parámetro a la URL -->
<a asp-action="Edit" asp-route-id="@paciente.Id">Editar</a>
<!-- Genera: <a href="/Pacientes/Edit/5">Editar</a> -->
```

### ¿Por qué usar Tag Helpers en vez de escribir la URL a mano?

Si cambias el nombre del controlador, la URL cambia. Con Tag Helpers,
ASP.NET actualiza los links automáticamente. Sin Tag Helpers,
tendrías que buscar y cambiar cada URL manualmente.

---

## ¿Cómo funciona el Layout?

### La analogía de la plantilla de revista

Una revista tiene siempre el mismo encabezado y pie de página,
pero el contenido de cada artículo es diferente.
`_Layout.cshtml` es esa plantilla. `@RenderBody()` es el espacio
donde va el contenido de cada página.

```html
<!-- Views/Shared/_Layout.cshtml — la plantilla común -->
<!DOCTYPE html>
<html>
<head>
    <title>@ViewData["Title"] - Clínica Médica</title>
    <link rel="stylesheet" href="~/lib/bootstrap/css/bootstrap.min.css" />
</head>
<body>
    <nav class="navbar navbar-dark bg-dark">
        <!-- menú de navegación -->
    </nav>

    <div class="container mt-4">
        @RenderBody()   <!-- AQUÍ va el contenido de cada vista -->
    </div>

    <script src="~/lib/jquery/dist/jquery.min.js"></script>
    @RenderSection("Scripts", required: false)
</body>
</html>
```

```html
<!-- Views/Pacientes/Index.cshtml — solo el contenido específico -->
@{
    ViewData["Title"] = "Pacientes";
    Layout = "_Layout";   <!-- usa la plantilla -->
}

<h2>Lista de pacientes</h2>
<!-- solo el contenido de esta página -->
```

---

## Resumen visual

```
Navegador  →  Controlador  →  Modelo (BD)
                    ↓
               Vista (.cshtml)
                    ↓
Navegador  ←  HTML generado
```

> Cuando algo no funciona, pregúntate:
> ¿Es un problema del Modelo (datos)? ¿De la Vista (HTML)? ¿Del Controlador (lógica)?
> Eso te ayuda a encontrar el error más rápido.

---

[← Volver al README](../README.md) | [Siguiente: Crear el proyecto →](./02-crear-proyecto.md)
