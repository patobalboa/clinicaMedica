
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using clinicaMedica.Models;
using clinicaMedica.Helpers; // Importamos el helper de paginación que creamos anteriormente en la carpeta Helpers


public class PacientesController : Controller
{
    private readonly ClinicaMedicaContext _context;

    public PacientesController(ClinicaMedicaContext context)
    {
        _context = context;
    }

    // GET: PACIENTES
    public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)    
    {
        // Para crear los toggles (▲ / ▼) 

        // ViewData["order"] = string.IsNullOrEmpty(sortOrder) ? "nombre_desc" : "";

        // Creamos los toggles (▲ / ▼)  para cada columno 
        ViewData["NombreSortParm"] = sortOrder == "Nombre" ? "nombre_desc" : "Nombre";
        ViewData["ApellidoSortParm"] = sortOrder == "Apellido" ? "apellido_desc" : "Apellido";
        ViewData["EmailSortParm"] = sortOrder == "Email" ? "email_desc" : "Email";


        if (searchString != null)
        {
            pageNumber = 1; // Si el usuario hizo una búsqueda nueva, volvemos a la página 1
        }
        else
        {
            searchString = currentFilter; // Si no hizo una búsqueda nueva, mantenemos el filtro actual
        }
    


        // El query base
        var pacientes = from p in _context.Pacientes select p;

        if(!string.IsNullOrEmpty(searchString))
        {
            pacientes = pacientes.Where(p => p.Nombre.Contains(searchString) || p.Apellido.Contains(searchString));
        }
    

        // Aplicar el ordenamiento según el sortOrder
        switch (sortOrder)
        {
            case "Nombre":
                pacientes = pacientes.OrderBy(p => p.Nombre);
                break;
            case "nombre_desc":
                pacientes = pacientes.OrderByDescending(p => p.Nombre);
                break;
            case "Apellido":
                pacientes = pacientes.OrderBy(p => p.Apellido);
                break;
            case "apellido_desc":
                pacientes = pacientes.OrderByDescending(p => p.Apellido);
                break;
            case "Email": 
                pacientes = pacientes.OrderBy(p => p.Email); break;
            case "email_desc": 
                pacientes = pacientes.OrderByDescending(p => p.Email); break;
            default: // Orden por defecto
                pacientes = pacientes.OrderBy(p => p.Apellido);
                break;
        }

        int pageSize = 10; // Cantidad de registros por página
        // El AsNoTracking() se usa para mejorar el rendimiento cuando solo queremos leer los datos sin modificarlos
        return View(await PaginatedList<Paciente>.CreateAsync(pacientes.AsNoTracking(), pageNumber ?? 1, pageSize));
    }

    // GET: PACIENTES/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var paciente = await _context.Pacientes
            .FirstOrDefaultAsync(m => m.Id == id);
        if (paciente == null)
        {
            return NotFound();
        }

        return View(paciente);
    }

    // GET: PACIENTES/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: PACIENTES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Nombre,Apellido,FechaNacimiento,Telefono,Email,Direccion,Foto,Cita")] Paciente paciente, IFormFile? fotoArchivo)
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

    // GET: PACIENTES/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente == null)
        {
            return NotFound();
        }
        return View(paciente);
    }

    // POST: PACIENTES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Nombre,Apellido,FechaNacimiento,Telefono,Email,Direccion,Foto,Cita")] Paciente paciente, IFormFile? fotoArchivo)
    {
        if (id != paciente.Id) return NotFound();

        if (fotoArchivo != null && fotoArchivo.Length > 0)
        {
            // Subió foto nueva → guardarla igual que en Create

            string extension = Path.GetExtension(fotoArchivo.FileName);
            string nombreUnico = Guid.NewGuid().ToString() + extension; // Se usa para evitar que dos fotos se llamen igual, ej: "jash12314-123124j-123123.jpg"
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

    // GET: PACIENTES/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var paciente = await _context.Pacientes
            .FirstOrDefaultAsync(m => m.Id == id);
        if (paciente == null)
        {
            return NotFound();
        }

        return View(paciente);
    }

    // POST: PACIENTES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente != null)
        {
            _context.Pacientes.Remove(paciente);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool PacienteExists(int? id)
    {
        return _context.Pacientes.Any(e => e.Id == id);
    }
}
