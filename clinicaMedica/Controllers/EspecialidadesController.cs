
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using clinicaMedica.Models;

public class EspecialidadesController : Controller
{
    private readonly ClinicaMedicaContext _context;

    public EspecialidadesController(ClinicaMedicaContext context)
    {
        _context = context;
    }

    // GET: ESPECIALIDADS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Especialidads.ToListAsync());
    }

    // GET: ESPECIALIDADS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var especialidad = await _context.Especialidads
            .FirstOrDefaultAsync(m => m.Id == id);
        if (especialidad == null)
        {
            return NotFound();
        }

        return View(especialidad);
    }

    // GET: ESPECIALIDADS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: ESPECIALIDADS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Descripcion,Cita")] Especialidad especialidad)
    {
        if (ModelState.IsValid)
        {
            _context.Add(especialidad);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(especialidad);
    }

    // GET: ESPECIALIDADS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var especialidad = await _context.Especialidads.FindAsync(id);
        if (especialidad == null)
        {
            return NotFound();
        }
        return View(especialidad);
    }

    // POST: ESPECIALIDADS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Descripcion,Cita")] Especialidad especialidad)
    {
        if (id != especialidad.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(especialidad);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EspecialidadExists(especialidad.Id))
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
        return View(especialidad);
    }

    // GET: ESPECIALIDADS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var especialidad = await _context.Especialidads
            .FirstOrDefaultAsync(m => m.Id == id);
        if (especialidad == null)
        {
            return NotFound();
        }

        return View(especialidad);
    }

    // POST: ESPECIALIDADS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var especialidad = await _context.Especialidads.FindAsync(id);
        if (especialidad != null)
        {
            _context.Especialidads.Remove(especialidad);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool EspecialidadExists(int? id)
    {
        return _context.Especialidads.Any(e => e.Id == id);
    }
}
