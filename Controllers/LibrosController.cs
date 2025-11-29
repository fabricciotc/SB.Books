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
    public async Task<IActionResult> Create(Libro libro, IFormFile? imagen)
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
            string? imageUrl = null;
            if (imagen != null && imagen.Length > 0)
            {
                _logger.LogInformation("Subiendo imagen: {FileName}, Tamaño: {Size} bytes", 
                    imagen.FileName, imagen.Length);
                
                using var stream = imagen.OpenReadStream();
                imageUrl = await _supabaseService.SubirImagenAsync(
                    stream,
                    imagen.FileName,
                    imagen.ContentType
                );
                
                if (string.IsNullOrEmpty(imageUrl))
                {
                    _logger.LogWarning("No se pudo subir la imagen: {FileName}", imagen.FileName);
                    ModelState.AddModelError("ImagenUrl", "Error al subir la imagen. Verifique el formato (JPG, PNG, GIF, WEBP) y que el archivo no esté vacío.");
                    return View(libro);
                }
                
                _logger.LogInformation("Imagen subida exitosamente. URL: {Url}", imageUrl);
            }

            libro.ImagenUrl = imageUrl;
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
    public async Task<IActionResult> Edit(int id, Libro libro, IFormFile? imagen, bool? eliminarImagen)
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
            var libroExistente = await _supabaseService.ObtenerLibroPorIdAsync(id);
            if (libroExistente == null)
                return NotFound();

            if (eliminarImagen == true && !string.IsNullOrEmpty(libroExistente.ImagenUrl))
            {
                await _supabaseService.EliminarImagenAsync(libroExistente.ImagenUrl);
                libro.ImagenUrl = null;
            }

            string? nuevaImagenUrl = null;
            if (imagen != null && imagen.Length > 0)
            {
                _logger.LogInformation("Subiendo nueva imagen para edición: {FileName}, Tamaño: {Size} bytes", 
                    imagen.FileName, imagen.Length);
                
                if (!string.IsNullOrEmpty(libroExistente.ImagenUrl))
                {
                    await _supabaseService.EliminarImagenAsync(libroExistente.ImagenUrl);
                }

                using var stream = imagen.OpenReadStream();
                nuevaImagenUrl = await _supabaseService.SubirImagenAsync(
                    stream,
                    imagen.FileName,
                    imagen.ContentType
                );
                
                if (string.IsNullOrEmpty(nuevaImagenUrl))
                {
                    _logger.LogWarning("No se pudo subir la nueva imagen: {FileName}", imagen.FileName);
                    ModelState.AddModelError("ImagenUrl", "Error al subir la imagen. Verifique el formato (JPG, PNG, GIF, WEBP) y que el archivo no esté vacío.");
                    return View(libro);
                }

                _logger.LogInformation("Nueva imagen subida exitosamente. URL: {Url}", nuevaImagenUrl);
                libro.ImagenUrl = nuevaImagenUrl;
            }
            else if (eliminarImagen != true)
            {
                libro.ImagenUrl = libroExistente.ImagenUrl;
            }

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
            var libro = await _supabaseService.ObtenerLibroPorIdAsync(id);
            if (libro != null && !string.IsNullOrEmpty(libro.ImagenUrl))
            {
                await _supabaseService.EliminarImagenAsync(libro.ImagenUrl);
            }

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
