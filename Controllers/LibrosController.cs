using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SupabaseNET.Models;
using SupabaseNET.Services;

namespace SupabaseNET.Controllers;

[Authorize]
public class LibrosController : Controller
{
    private readonly ISupabaseService _supabaseService;
    private readonly ILogger<LibrosController> _logger;

    public LibrosController(ISupabaseService supabaseService, ILogger<LibrosController> logger)
    {
        _supabaseService = supabaseService ?? throw new ArgumentNullException(nameof(supabaseService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // GET: Libros
    public async Task<IActionResult> Index()
    {
        var libros = await _supabaseService.ObtenerLibrosAsync();
        return View(libros);
    }

    // GET: Libros/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var libro = await _supabaseService.ObtenerLibroPorIdAsync(id.Value);
        if (libro == null)
            return NotFound();

        return View(libro);
    }

    // GET: Libros/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Libros/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Libro libro)
    {
        try
        {
            libro.Validar();
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(libro);
        }

        if (!ModelState.IsValid)
        {
            return View(libro);
        }

        try
        {
            var success = await _supabaseService.CrearLibroAsync(libro);

            if (success)
            {
                TempData["SuccessMessage"] = "Libro creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Error al crear el libro.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear libro. Error: {Error}", ex.Message);
            ModelState.AddModelError(string.Empty, "Error al crear el libro. " + ex.Message);
        }

        return View(libro);
    }

    // GET: Libros/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var libro = await _supabaseService.ObtenerLibroPorIdAsync(id.Value);
        if (libro == null)
            return NotFound();

        return View(libro);
    }

    // POST: Libros/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Libro libro)
    {
        if (id != libro.Id)
            return NotFound();

        try
        {
            libro.Validar();
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(libro);
        }

        if (!ModelState.IsValid)
        {
            return View(libro);
        }

        try
        {
            var success = await _supabaseService.ActualizarLibroAsync(libro);

            if (success)
            {
                TempData["SuccessMessage"] = "Libro actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Error al actualizar el libro.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar libro con Id: {Id}", id);
            ModelState.AddModelError(string.Empty, "Error al actualizar el libro. " + ex.Message);
        }

        return View(libro);
    }

    // GET: Libros/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var libro = await _supabaseService.ObtenerLibroPorIdAsync(id.Value);
        if (libro == null)
            return NotFound();

        return View(libro);
    }

    // POST: Libros/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var success = await _supabaseService.EliminarLibroAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Libro eliminado exitosamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error al eliminar el libro.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar libro con Id: {Id}", id);
            TempData["ErrorMessage"] = "Error al eliminar el libro.";
        }

        return RedirectToAction(nameof(Index));
    }
}
