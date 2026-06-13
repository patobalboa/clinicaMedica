# 03 — Modelos, relaciones y CRUD automático

---

## ¿Qué son los modelos?

Un modelo es una clase C# que representa una tabla de la base de datos.
Cada propiedad de la clase corresponde a una columna.

```csharp
// Models/Paciente.cs — generado automáticamente por EF Core
public partial class Paciente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Apellido { get; set; } = null!;
    public DateOnly? FechaNacimiento { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Foto { get; set; }

    // Relación: un paciente tiene muchas citas
    public virtual ICollection<Citum> Cita { get; set; } = new List<Citum>();
}
```

> **¿Qué significa `null!`?**
> Le dice a C# "esta propiedad no puede ser null, te lo garantizo".
> Es la forma de evitar advertencias del compilador para campos obligatorios.

---

## Las relaciones entre tablas

### La analogía del álbum familiar

Un paciente puede tener muchas citas (como una persona puede tener muchas fotos en un álbum),
pero cada cita pertenece a un solo paciente.
Eso es una relación **uno a muchos**.

En el código:

```csharp
// Citum.cs — el lado "muchos" (cada cita pertenece a un paciente)
public partial class Citum
{
    public int Id { get; set; }
    public DateTime FechaHora { get; set; }
    public string Estado { get; set; } = null!;

    // Clave foránea: guarda el ID del paciente
    public int? PacienteId { get; set; }

    // Propiedad de navegación: permite acceder al objeto Paciente completo
    public virtual Paciente? Paciente { get; set; }
}

// Paciente.cs — el lado "uno" (un paciente tiene muchas citas)
public partial class Paciente
{
    // ...propiedades...

    // Colección de citas de este paciente
    public virtual ICollection<Citum> Cita { get; set; } = new List<Citum>();
}
```

### Cargar datos relacionados con Include

Sin `Include`, la relación llega vacía:

```csharp
// ❌ Sin Include — paciente.Nombre llega con datos, pero cita.Paciente es null
var citas = await _context.Cita.ToListAsync();

// ✅ Con Include — cita.Paciente tiene todos los datos del paciente
var citas = await _context.Cita
    .Include(c => c.Paciente)
    .Include(c => c.Medico)
    .Include(c => c.Especialidad)
    .ToListAsync();
```

---

## Generar el CRUD automático (scaffolding)

Visual Studio puede crear el controlador y las 4 vistas (Index, Create, Edit, Delete)
automáticamente para cada modelo.

### Pasos

1. Compilar el proyecto (`Ctrl+Shift+B`)
2. Clic derecho sobre la carpeta `Controllers`
3. **Agregar > Nuevo elemento con scaffolding**
4. Seleccionar **Controlador de MVC con vistas que usa Entity Framework**
5. Configurar:
   - **Clase de modelo:** `Paciente`
   - **Clase de contexto de datos:** `ClinicaMedicaContext`
   - Dejar marcadas las opciones de vistas
6. **Agregar**

Repite esto para: `Medico`, `Especialidad`, `Citum`, `Tratamiento`.

### Qué se genera

| Archivo | Función |
|---|---|
| `Controllers/PacientesController.cs` | Las 5 acciones: Index, Details, Create, Edit, Delete |
| `Views/Pacientes/Index.cshtml` | Lista de pacientes |
| `Views/Pacientes/Create.cshtml` | Formulario para crear |
| `Views/Pacientes/Edit.cshtml` | Formulario para editar |
| `Views/Pacientes/Delete.cshtml` | Confirmación para eliminar |
| `Views/Pacientes/Details.cshtml` | Ver detalle de un paciente |

---

## Entender el controlador generado

```csharp
// Controllers/PacientesController.cs — versión simplificada
public class PacientesController : Controller
{
    private readonly ClinicaMedicaContext _context;

    // El contexto se "inyecta" automáticamente — no necesitas crearlo
    public PacientesController(ClinicaMedicaContext context)
    {
        _context = context;
    }

    // GET: /Pacientes — muestra la lista
    public async Task<IActionResult> Index()
    {
        var pacientes = await _context.Pacientes.ToListAsync();
        return View(pacientes);
    }

    // GET: /Pacientes/Create — muestra el formulario vacío
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Pacientes/Create — recibe el formulario y guarda
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Paciente paciente)
    {
        if (ModelState.IsValid)
        {
            _context.Add(paciente);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(paciente);  // si hay errores, vuelve al formulario
    }
}
```

> **¿Qué es `[ValidateAntiForgeryToken]`?**
> Es una medida de seguridad automática. Evita que alguien envíe
> datos desde otro sitio web haciéndose pasar por tu aplicación.
> Déjalo siempre en los métodos POST.

---

## Agregar el menú de navegación

Abre `Views/Shared/_Layout.cshtml` y agrega los links en la barra de navegación:

```html
<ul class="navbar-nav">
    <li class="nav-item">
        <a class="nav-link" asp-controller="Pacientes" asp-action="Index">Pacientes</a>
    </li>
    <li class="nav-item">
        <a class="nav-link" asp-controller="Medicos" asp-action="Index">Médicos</a>
    </li>
    <li class="nav-item">
        <a class="nav-link" asp-controller="Especialidads" asp-action="Index">Especialidades</a>
    </li>
    <li class="nav-item">
        <a class="nav-link" asp-controller="Cita" asp-action="Index">Citas</a>
    </li>
    <li class="nav-item">
        <a class="nav-link" asp-controller="Tratamientos" asp-action="Index">Tratamientos</a>
    </li>
</ul>
```

---

## Resultado esperado

Al terminar, debes poder:
- Abrir la aplicación y ver el menú con todos los módulos
- Crear, editar y eliminar pacientes, médicos, especialidades, citas y tratamientos
- Ver los datos cargados desde la base de datos

---

[← Guía 02](./02-crear-proyecto.md) | [Siguiente: Validaciones →](./04-validaciones-formularios.md)
