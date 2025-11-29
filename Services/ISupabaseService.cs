using SupabaseNET.Models;

namespace SupabaseNET.Services;

public interface ISupabaseService
{
    Task<bool> RegistrarUsuarioAsync(string email, string password);
    Task<bool> IniciarSesionAsync(string email, string password);
    Task<bool> CerrarSesionAsync();
    Task<string?> ObtenerUsuarioIdAsync();
    Task<bool> UsuarioAutenticadoAsync();

    // CRUD de Libros
    Task<List<Libro>> ObtenerLibrosAsync();
    Task<Libro?> ObtenerLibroPorIdAsync(int id);
    Task<bool> CrearLibroAsync(Libro libro);
    Task<bool> ActualizarLibroAsync(Libro libro);
    Task<bool> EliminarLibroAsync(int id);
}
