
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using clinicaMedica.Models;

public class MedicosController : Controller
{
    private readonly ClinicaMedicaContext _context;

    public MedicosController(ClinicaMedicaContext context)
    {
        _context = context;
    }

    // GET: MEDICOS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Medicos.ToListAsync());
    }

    // GET: MEDICOS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var medico = await _context.Medicos
            .FirstOrDefaultAsync(m => m.Id == id);
        if (medico == null)
        {
            return NotFound();
        }

        return View(medico);
    }

    // GET: MEDICOS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: MEDICOS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Nombre,Apellido,Especialidad,Telefono,Email,Cita")] Medico medico)
    {
        if (ModelState.IsValid)
        {
            _context.Add(medico);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(medico);
    }

    // GET: MEDICOS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var medico = await _context.Medicos.FindAsync(id);
        if (medico == null)
        {
            return NotFound();
        }
        return View(medico);
    }

    // POST: MEDICOS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Nombre,Apellido,Especialidad,Telefono,Email,Cita")] Medico medico)
    {
        if (id != medico.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(medico);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MedicoExists(medico.Id))
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
        return View(medico);
    }

    // GET: MEDICOS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var medico = await _context.Medicos
            .FirstOrDefaultAsync(m => m.Id == id);
        if (medico == null)
        {
            return NotFound();
        }

        return View(medico);
    }

    // POST: MEDICOS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var medico = await _context.Medicos.FindAsync(id);
        if (medico != null)
        {
            _context.Medicos.Remove(medico);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool MedicoExists(int? id)
    {
        return _context.Medicos.Any(e => e.Id == id);
    }
}
