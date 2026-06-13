# 08 — Login, Registro y Roles con ASP.NET Core Identity

> Esta guía es para la Evaluación 3.
> La EV2 debe estar funcionando antes de empezar.

---

## ¿Qué es Identity?

Identity es el sistema de usuarios que viene incluido en ASP.NET Core.
Te da registro, login, logout, roles y más, sin tener que programarlo desde cero.

---

## Paso 1 — Instalar paquetes

```powershell
Install-Package Microsoft.AspNetCore.Identity.EntityFrameworkCore
Install-Package Microsoft.AspNetCore.Identity.UI
```

---

## Paso 2 — Crear el ApplicationUser

Crea `Models/ApplicationUser.cs`:

```csharp
using Microsoft.AspNetCore.Identity;

namespace ClinicaMedica.Models;

public class ApplicationUser : IdentityUser
{
    public string? NombreCompleto { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public bool Activo { get; set; } = true;
}
```

---

## Paso 3 — Actualizar el DbContext

Abre `Models/ClinicaMedicaContext.cs` y cambia la clase base:

```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ClinicaMedica.Models;

// ANTES: public partial class ClinicaMedicaContext : DbContext
// DESPUÉS:
public partial class ClinicaMedicaContext : IdentityDbContext<ApplicationUser>
{
    public ClinicaMedicaContext(DbContextOptions<ClinicaMedicaContext> options)
        : base(options) { }

    // Agrega esto dentro del OnModelCreating existente:
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);  // DEBE estar primero
        // ... el resto del OnModelCreating que ya tenías
    }

    // tus DbSets existentes...
    public virtual DbSet<Paciente> Pacientes { get; set; }
    // etc.
}
```

---

## Paso 4 — Configurar Identity en Program.cs

```csharp
using Microsoft.AspNetCore.Identity;
using ClinicaMedica.Models;

// Agregar ANTES de builder.Build()
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opciones =>
{
    opciones.Password.RequireDigit           = true;
    opciones.Password.RequiredLength         = 8;
    opciones.Password.RequireUppercase       = true;
    opciones.Password.RequireNonAlphanumeric = true;
    opciones.SignIn.RequireConfirmedEmail     = false;
})
.AddEntityFrameworkStores<ClinicaMedicaContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opciones =>
{
    opciones.LoginPath        = "/Account/Login";
    opciones.LogoutPath       = "/Account/Logout";
    opciones.AccessDeniedPath = "/Account/AccessDenied";
});
```

Y más abajo, en el orden de middlewares:

```csharp
var app = builder.Build();

app.UseStaticFiles();       // primero — para que los CSS funcionen sin login
app.UseAuthentication();    // segundo — ¿quién eres?
app.UseAuthorization();     // tercero — ¿qué puedes hacer?
```

> **⚠️ El orden importa.** Si pones `UseAuthorization` antes de `UseAuthentication`,
> los roles no funcionan aunque no dé error.

---

## Paso 5 — Crear la migración de Identity

```powershell
dotnet ef migrations add AgregarIdentity
dotnet ef database update
```

Esto crea las tablas: `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, etc.

---

## Paso 6 — ViewModels para Register y Login

Crea la carpeta `Models/ViewModels/` y los archivos:

**`RegisterViewModel.cs`:**
```csharp
using System.ComponentModel.DataAnnotations;

namespace ClinicaMedica.Models.ViewModels;

public class RegisterViewModel
{
    [Required][Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required][EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required][StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)][Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)][Display(Name = "Confirmar contraseña")]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
```

**`LoginViewModel.cs`:**
```csharp
using System.ComponentModel.DataAnnotations;

namespace ClinicaMedica.Models.ViewModels;

public class LoginViewModel
{
    [Required][EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required][DataType(DataType.Password)][Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Recordarme")]
    public bool RememberMe { get; set; }
}
```

---

## Paso 7 — AccountController

Crea `Controllers/AccountController.cs`:

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ClinicaMedica.Models;
using ClinicaMedica.Models.ViewModels;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser>   _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager,
                             SignInManager<ApplicationUser> signInManager)
    {
        _userManager   = userManager;
        _signInManager = signInManager;
    }

    public IActionResult Register() => View();

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        var usuario = new ApplicationUser
        {
            UserName       = modelo.Email,
            Email          = modelo.Email,
            NombreCompleto = modelo.NombreCompleto,
        };

        var resultado = await _userManager.CreateAsync(usuario, modelo.Password);

        if (resultado.Succeeded)
        {
            await _userManager.AddToRoleAsync(usuario, "Usuario");  // rol por defecto
            await _signInManager.SignInAsync(usuario, isPersistent: false);
            return RedirectToAction("Index", "Pacientes");
        }

        foreach (var error in resultado.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(modelo);
    }

    public IActionResult Login() => View();

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        var resultado = await _signInManager.PasswordSignInAsync(
            modelo.Email, modelo.Password, modelo.RememberMe, lockoutOnFailure: true);

        if (resultado.Succeeded)
            return RedirectToAction("Index", "Pacientes");

        if (resultado.IsLockedOut)
            ModelState.AddModelError("", "Cuenta bloqueada. Intenta en 5 minutos.");
        else
            ModelState.AddModelError("", "Email o contraseña incorrectos.");

        return View(modelo);
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
}
```

---

## Paso 8 — Crear roles y usuario admin al iniciar

Al final de `Program.cs`, antes de `app.Run()`:

```csharp
// Seed de roles y usuario administrador
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Crear los 3 roles
    string[] roles = { "Administrador", "Supervisor", "Usuario" };
    foreach (var rol in roles)
        if (!await roleManager.RoleExistsAsync(rol))
            await roleManager.CreateAsync(new IdentityRole(rol));

    // Crear usuario administrador por defecto
    var adminEmail = "admin@clinica.cl";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser
        {
            UserName       = adminEmail,
            Email          = adminEmail,
            NombreCompleto = "Administrador del Sistema"
        };
        await userManager.CreateAsync(admin, "Admin@12345");
        await userManager.AddToRoleAsync(admin, "Administrador");
    }
}

app.Run();
```

---

## Paso 9 — Proteger los controladores

```csharp
// Solo usuarios autenticados
[Authorize]
public class PacientesController : Controller { ... }

// Solo Administrador
[Authorize(Roles = "Administrador")]
public IActionResult Delete(int? id) { ... }

// Administrador o Supervisor
[Authorize(Roles = "Administrador,Supervisor")]
public IActionResult Create() { ... }
```

### Mostrar u ocultar botones en las vistas según el rol

```html
@* Mostrar "Eliminar" solo al Administrador *@
@if (User.IsInRole("Administrador"))
{
    <a asp-action="Delete" asp-route-id="@item.Id" class="btn btn-danger btn-sm">Eliminar</a>
}

@* Mostrar "Editar" a Administrador y Supervisor *@
@if (User.IsInRole("Administrador") || User.IsInRole("Supervisor"))
{
    <a asp-action="Edit" asp-route-id="@item.Id" class="btn btn-warning btn-sm">Editar</a>
}
```

---

## Paso 10 — Mostrar el usuario en el Layout

```html
@* Views/Shared/_Layout.cshtml *@
@using Microsoft.AspNetCore.Identity
@inject SignInManager<ApplicationUser> SignInManager
@inject UserManager<ApplicationUser>   UserManager

@if (SignInManager.IsSignedIn(User))
{
    <span class="navbar-text me-3">@UserManager.GetUserName(User)</span>
    <form asp-controller="Account" asp-action="Logout" method="post" class="d-inline">
        <button type="submit" class="btn btn-outline-light btn-sm">Salir</button>
    </form>
}
else
{
    <a asp-controller="Account" asp-action="Login"    class="btn btn-outline-light btn-sm me-1">Entrar</a>
    <a asp-controller="Account" asp-action="Register" class="btn btn-light btn-sm">Registrarse</a>
}
```

---

[← Guía 07](./07-exportar-excel-pdf.md) | [Siguiente: Filtros y ordenación →](./09-filtros-ordenacion.md)
