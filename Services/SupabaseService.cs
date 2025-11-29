using Supabase;
using SupabaseSession = Supabase.Gotrue.Session;
using SupabaseNET.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace SupabaseNET.Services;

public class SupabaseService : ISupabaseService
{
    private readonly Supabase.Client _client;
    private readonly ILogger<SupabaseService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string SessionAccessToken = "SupabaseAccessToken";
    private const string SessionRefreshToken = "SupabaseRefreshToken";

    public SupabaseService(Supabase.Client client, ILogger<SupabaseService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _client = client;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    private void GuardarTokensEnSesion(SupabaseSession session)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Session != null && session != null)
        {
            if (!string.IsNullOrEmpty(session.AccessToken))
            {
                httpContext.Session.SetString(SessionAccessToken, session.AccessToken);
            }
            if (!string.IsNullOrEmpty(session.RefreshToken))
            {
                httpContext.Session.SetString(SessionRefreshToken, session.RefreshToken);
            }
        }
    }

    private string? ObtenerUserIdDesdeClaims()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private async Task AsegurarSesionSupabase()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Session == null || httpContext.User?.Identity?.IsAuthenticated != true)
            return;

        if (_client.Auth.CurrentUser != null)
            return;

        var accessToken = httpContext.Session.GetString(SessionAccessToken);
        if (string.IsNullOrEmpty(accessToken))
            return;

        var refreshToken = httpContext.Session.GetString(SessionRefreshToken) ?? string.Empty;

        try
        {
            await _client.Auth.SetSession(accessToken, refreshToken);
        }
        catch
        {
            // Si falla, continuar
        }
    }

    public async Task<bool> RegistrarUsuarioAsync(string email, string password)
    {
        try
        {
            var response = await _client.Auth.SignUp(email, password);
            if (response?.User != null)
            {
                var session = _client.Auth.CurrentSession;
                if (session != null)
                {
                    GuardarTokensEnSesion(session);
                }
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar usuario: {Email}", email);
            return false;
        }
    }

    public async Task<bool> IniciarSesionAsync(string email, string password)
    {
        try
        {
            var response = await _client.Auth.SignInWithPassword(email, password);
            if (response?.User != null)
            {
                var session = _client.Auth.CurrentSession;
                if (session != null)
                {
                    GuardarTokensEnSesion(session);
                }
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al iniciar sesión: {Email}", email);
            return false;
        }
    }

    public async Task<bool> CerrarSesionAsync()
    {
        try
        {
            await _client.Auth.SignOut();
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Session != null)
            {
                httpContext.Session.Remove(SessionAccessToken);
                httpContext.Session.Remove(SessionRefreshToken);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cerrar sesión");
            return false;
        }
    }

    public Task<string?> ObtenerUsuarioIdAsync()
    {
        var userId = ObtenerUserIdDesdeClaims();
        if (!string.IsNullOrEmpty(userId))
            return Task.FromResult<string?>(userId);

        return Task.FromResult<string?>(_client.Auth.CurrentUser?.Id);
    }

    public Task<bool> UsuarioAutenticadoAsync()
    {
        var isAuthenticated = _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
        return Task.FromResult(isAuthenticated);
    }

    public async Task<List<Libro>> ObtenerLibrosAsync()
    {
        try
        {
            await AsegurarSesionSupabase();
            var userId = await ObtenerUsuarioIdAsync();
            if (string.IsNullOrEmpty(userId))
                return new List<Libro>();

            var response = await _client
                .From<Libro>()
                .Where(x => x.UsuarioId == userId)
                .Get();

            return response.Models ?? new List<Libro>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener libros");
            return new List<Libro>();
        }
    }

    public async Task<Libro?> ObtenerLibroPorIdAsync(int id)
    {
        try
        {
            await AsegurarSesionSupabase();
            var userId = await ObtenerUsuarioIdAsync();
            if (string.IsNullOrEmpty(userId))
                return null;

            var response = await _client
                .From<Libro>()
                .Where(x => x.Id == id && x.UsuarioId == userId)
                .Single();

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener libro con Id: {Id}", id);
            return null;
        }
    }

    public async Task<bool> CrearLibroAsync(Libro libro)
    {
        try
        {
            await AsegurarSesionSupabase();
            var userId = await ObtenerUsuarioIdAsync();
            if (string.IsNullOrEmpty(userId))
                return false;

            libro.UsuarioId = userId;
            libro.FechaCreacion = DateTime.UtcNow;

            var response = await _client
                .From<Libro>()
                .Insert(libro);

            return response != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear libro");
            return false;
        }
    }

    public async Task<bool> ActualizarLibroAsync(Libro libro)
    {
        try
        {
            await AsegurarSesionSupabase();
            var userId = await ObtenerUsuarioIdAsync();
            if (string.IsNullOrEmpty(userId))
                return false;

            libro.UsuarioId = userId;

            var response = await _client
                .From<Libro>()
                .Where(x => x.Id == libro.Id && x.UsuarioId == userId)
                .Set(x => x.Titulo, libro.Titulo)
                .Set(x => x.Autor, libro.Autor)
                .Set(x => x.Isbn, libro.Isbn)
                .Set(x => x.AnioPublicacion, libro.AnioPublicacion)
                .Set(x => x.Editorial, libro.Editorial)
                .Update();

            return response?.Models?.Count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar libro con Id: {Id}", libro.Id);
            return false;
        }
    }

    public async Task<bool> EliminarLibroAsync(int id)
    {
        try
        {
            await AsegurarSesionSupabase();
            var userId = await ObtenerUsuarioIdAsync();
            if (string.IsNullOrEmpty(userId))
                return false;

            await _client
                .From<Libro>()
                .Where(x => x.Id == id && x.UsuarioId == userId)
                .Delete();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar libro con Id: {Id}", id);
            return false;
        }
    }
}
