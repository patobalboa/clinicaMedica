# 🏥 Taller de Recuperación — Clínica Médica
## ASP.NET Core MVC con C#

> **Para el alumno:** Este proyecto ya tiene la base de datos creada.
> Tu trabajo es construir la aplicación web paso a paso siguiendo las guías.
> Cada guía es independiente — si tienes problemas con una parte, puedes pedir ayuda sin que afecte el resto.

---

## ¿Qué vamos a construir?

Un sistema web para administrar una clínica médica con:
- Pacientes, Médicos, Especialidades, Citas y Tratamientos
- Subida de fotos de pacientes
- Exportación a Excel y PDF
- Login y roles de usuario (Evaluación 3)
- Tablas con filtros y búsqueda (Evaluación 3)

---

## Índice de guías

| # | Guía | Qué aprendes | Para qué evaluación |
|---|------|--------------|-------------------|
| 01 | [Cómo funciona MVC](./guias/01-mvc-explicado.md) | MVC, async/await, Tag Helpers | Base de todo |
| 02 | [Crear el proyecto](./guias/02-crear-proyecto.md) | Crear proyecto, instalar EF Core, conectar BD | EV2 |
| 03 | [Modelos y scaffolding](./guias/03-modelos-scaffolding.md) | Generar modelos desde la BD, crear CRUD automático | EV2 |
| 04 | [Validaciones y formularios](./guias/04-validaciones-formularios.md) | DataAnnotations, ModelState, Tag Helpers | EV2 |
| 05 | [Subir imágenes](./guias/05-imagenes-iformfile.md) | IFormFile, guardar y mostrar fotos | EV2 |
| 06 | [Paginación con X.PagedList](./guias/06-paginacion.md) | X.PagedList.Mvc.Core | EV2 |
| 07 | [Exportar Excel y PDF](./guias/07-exportar-excel-pdf.md) | ClosedXML, QuestPDF | EV2 |
| 08 | [Login y Roles (Identity)](./guias/08-identity-login-roles.md) | Register, Login, Logout, Roles | EV3 |
| 09 | [Filtros y ordenación](./guias/09-filtros-ordenacion.md) | Buscar, ordenar columnas, combinar con paginación | EV3 |

---

## ¿Qué necesito tener instalado?

- Visual Studio Community 2022 (con carga de trabajo **ASP.NET y desarrollo web**)
- SQL Server Express
- SQL Server Management Studio (SSMS)

---

## La base de datos del proyecto

Ya tienes el script SQL. Las tablas son:

```
pacientes     → id, nombre, apellido, fecha_nacimiento, telefono, email, foto
medicos       → id, nombre, apellido, especialidad, telefono, email
especialidad  → id, descripcion
citas         → id, paciente_id, medico_id, especialidad_id, fecha_hora, estado
tratamientos  → id, cita_id, notas, fecha_registro
```

---

## Antes de empezar

1. Ejecuta el script SQL en SSMS para crear la base de datos `clinica_medica`
2. Lee la guía 01 para entender cómo funciona todo
3. Sigue las guías en orden — cada una se apoya en la anterior

---

## Qué necesita cada evaluación

### Evaluación 2 (guías 01 → 07)
- Proyecto MVC con estructura correcta
- 5 modelos con CRUD completo
- Foto en pacientes (IFormFile)
- Paginación en todas las vistas Index
- Formularios con validación
- Exportar Excel y PDF

### Evaluación 3 (guías 08 → 09, más todo lo de EV2)
- Login, Registro y Logout (Identity)
- 3 roles: Administrador, Supervisor, Usuario
- Panel de administración de usuarios
- Filtros y búsqueda en todas las tablas
- Ordenación por columnas

---

> 💡 **Consejo:** No copies y pegues el código sin leerlo.
> En clases te harán preguntas sobre lo que hiciste.
> Entiende cada parte — las guías están escritas para ayudarte a entender, no solo a copiar.
