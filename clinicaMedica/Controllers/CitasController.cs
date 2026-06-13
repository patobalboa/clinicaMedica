
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using clinicaMedica.Models;

public class CitasController : Controller
{
    private readonly ClinicaMedicaContext _context;

    public CitasController(ClinicaMedicaContext context)
    {
        _context = context;
    }

    // GET: CITAS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Citas.ToListAsync());
    }

    // GET: CITAS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var cita = await _context.Citas
            .FirstOrDefaultAsync(m => m.Id == id);
        if (cita == null)
        {
            return NotFound();
        }

        return View(cita);
    }

    // GET: CITAS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: CITAS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,PacienteId,MedicoId,EspecialidadId,FechaHora,Estado,Especialidad,Medico,Paciente,Tratamientos")] Cita cita)
    {
        if (ModelState.IsValid)
        {
            _context.Add(cita);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(cita);
    }

    // GET: CITAS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var cita = await _context.Citas.FindAsync(id);
        if (cita == null)
        {
            return NotFound();
        }
        return View(cita);
    }

    // POST: CITAS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,PacienteId,MedicoId,EspecialidadId,FechaHora,Estado,Especialidad,Medico,Paciente,Tratamientos")] Cita cita)
    {
        if (id != cita.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(cita);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CitaExists(cita.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(cita);
    }

    // GET: CITAS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var cita = await _context.Citas
            .FirstOrDefaultAsync(m => m.Id == id);
        if (cita == null)
        {
            return NotFound();
        }

        return View(cita);
    }

    // POST: CITAS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var cita = await _context.Citas.FindAsync(id);
        if (cita != null)
        {
            _context.Citas.Remove(cita);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CitaExists(int? id)
    {
        return _context.Citas.Any(e => e.Id == id);
    }
}
