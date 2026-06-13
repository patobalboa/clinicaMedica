
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using clinicaMedica.Models;

public class TratamientosController : Controller
{
    private readonly ClinicaMedicaContext _context;

    public TratamientosController(ClinicaMedicaContext context)
    {
        _context = context;
    }

    // GET: TRATAMIENTOS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Tratamientos.ToListAsync());
    }

    // GET: TRATAMIENTOS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var tratamiento = await _context.Tratamientos
            .FirstOrDefaultAsync(m => m.Id == id);
        if (tratamiento == null)
        {
            return NotFound();
        }

        return View(tratamiento);
    }

    // GET: TRATAMIENTOS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: TRATAMIENTOS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,CitaId,Notas,FechaRegistro,Cita")] Tratamiento tratamiento)
    {
        if (ModelState.IsValid)
        {
            _context.Add(tratamiento);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(tratamiento);
    }

    // GET: TRATAMIENTOS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var tratamiento = await _context.Tratamientos.FindAsync(id);
        if (tratamiento == null)
        {
            return NotFound();
        }
        return View(tratamiento);
    }

    // POST: TRATAMIENTOS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,CitaId,Notas,FechaRegistro,Cita")] Tratamiento tratamiento)
    {
        if (id != tratamiento.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(tratamiento);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TratamientoExists(tratamiento.Id))
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
        return View(tratamiento);
    }

    // GET: TRATAMIENTOS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var tratamiento = await _context.Tratamientos
            .FirstOrDefaultAsync(m => m.Id == id);
        if (tratamiento == null)
        {
            return NotFound();
        }

        return View(tratamiento);
    }

    // POST: TRATAMIENTOS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var tratamiento = await _context.Tratamientos.FindAsync(id);
        if (tratamiento != null)
        {
            _context.Tratamientos.Remove(tratamiento);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool TratamientoExists(int? id)
    {
        return _context.Tratamientos.Any(e => e.Id == id);
    }
}
