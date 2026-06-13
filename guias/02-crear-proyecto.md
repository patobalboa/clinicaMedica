# 02 — Crear el proyecto y conectar la base de datos

---

## Paso 1 — Crear el proyecto en Visual Studio

1. Abrir Visual Studio → **Crear un proyecto**
2. Buscar y seleccionar **Aplicación web de ASP.NET Core (Modelo-Vista-Controlador)**
3. Nombre del proyecto: `ClinicaMedica`
4. Versión de .NET: la que indique el docente (.NET 8)
5. Mantener habilitado HTTPS → **Crear**

---

## Paso 2 — Instalar Entity Framework Core

Ir a **Herramientas > Administrador de paquetes NuGet > Consola del Administrador de paquetes**

Ejecutar estos comandos uno por uno:

```powershell
Install-Package Microsoft.EntityFrameworkCore.SqlServer
Install-Package Microsoft.EntityFrameworkCore.Tools
Install-Package Microsoft.EntityFrameworkCore.Design
```

| Paquete | Para qué sirve |
|---|---|
| `SqlServer` | Conectarse a SQL Server |
| `Tools` | Usar el comando `Scaffold-DbContext` |
| `Design` | Necesario para generar código en tiempo de desarrollo |

---

## Paso 3 — Configurar la cadena de conexión

Abre el archivo `appsettings.json` y agrega tu conexión:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Initial Catalog=clinica_medica;Trusted_Connection=True;Encrypt=False;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> **¿Qué significa cada parte?**
> - `Server=.\\SQLEXPRESS` → tu SQL Server local
> - `Initial Catalog=clinica_medica` → el nombre de la base de datos
> - `Trusted_Connection=True` → usa tu usuario de Windows (no necesitas contraseña)
> - `Encrypt=False` → para desarrollo local, no necesita cifrado

---

## Paso 4 — Generar los modelos desde la base de datos

Este proceso se llama **Database First** — EF Core lee tus tablas y genera las clases C# automáticamente.

En la Consola del Administrador de paquetes:

```powershell
Scaffold-DbContext "Server=.\SQLEXPRESS;Initial Catalog=clinica_medica;Trusted_Connection=True;Encrypt=False;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Force
```

Esto genera en la carpeta `Models/`:
- `Paciente.cs`
- `Medico.cs`
- `Especialidad.cs`
- `Citum.cs` ← ojo, genera "Citum" en vez de "Cita" (lo veremos abajo)
- `Tratamiento.cs`
- `ClinicaMedicaContext.cs` ← el DbContext

> **Problema común: el nombre "Citum"**
> EF Core a veces pluraliza mal los nombres en español.
> Si genera `Citum`, puedes dejarlo así o renombrarlo a `Cita`
> (recuerda actualizar el DbContext y todas las referencias).

---

## Paso 5 — Mover el DbContext a Program.cs

El scaffolding guarda la cadena de conexión dentro del DbContext.
Hay que moverla a `Program.cs` para que use `appsettings.json`.

Abre `Models/ClinicaMedicaContext.cs` y **elimina** el método `OnConfiguring`:

```csharp
// ELIMINAR este método completo del ClinicaMedicaContext.cs
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder.UseSqlServer("...");
```

Luego abre `Program.cs` y agrega el registro del contexto:

```csharp
// Program.cs
using ClinicaMedica.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Agregar esta línea ANTES de builder.Build()
builder.Services.AddDbContext<ClinicaMedicaContext>(opciones =>
    opciones.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();
```

---

## Paso 6 — Verificar que compila

Presiona `Ctrl+Shift+B` para compilar. Si hay errores:

| Error frecuente | Solución |
|---|---|
| `Encrypt=Fase` | Corregir a `Encrypt=False` |
| Namespace no encontrado | Agregar el `using` correspondiente en Program.cs |
| Error de conexión al compilar | Verificar que SQL Server Express esté corriendo |

---

## Resultado esperado

Al terminar este paso, tu proyecto debe:
- Compilar sin errores (`Ctrl+Shift+B`)
- Tener la carpeta `Models/` con 6 archivos (5 modelos + el contexto)
- Tener `appsettings.json` con la cadena de conexión

---

[← Guía 01](./01-mvc-explicado.md) | [Siguiente: Modelos y CRUD →](./03-modelos-scaffolding.md)
