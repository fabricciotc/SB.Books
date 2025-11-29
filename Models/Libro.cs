using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SupabaseNET.Models;

[Table("libros")]
public class Libro : BaseModel
{
    [PrimaryKey("id")]
    [Column("id")]
    public int Id { get; set; }

    [Required(ErrorMessage = "El título es obligatorio")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "El título debe tener entre 1 y 255 caracteres")]
    [Display(Name = "Título")]
    [Column("titulo")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El autor es obligatorio")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "El autor debe tener entre 1 y 255 caracteres")]
    [Display(Name = "Autor")]
    [Column("autor")]
    public string Autor { get; set; } = string.Empty;

    [StringLength(13, ErrorMessage = "El ISBN debe tener máximo 13 caracteres")]
    [Display(Name = "ISBN")]
    [Column("isbn")]
    public string? Isbn { get; set; }

    [Range(1000, 9999, ErrorMessage = "Ingrese un año válido entre 1000 y 9999")]
    [Display(Name = "Año de Publicación")]
    [Column("anio_publicacion")]
    public int? AnioPublicacion { get; set; }

    [StringLength(255, ErrorMessage = "La editorial no puede exceder 255 caracteres")]
    [Display(Name = "Editorial")]
    [Column("editorial")]
    public string? Editorial { get; set; }

    [Column("fecha_creacion")]
    public DateTime? FechaCreacion { get; set; }

    [Column("usuario_id")]
    public string? UsuarioId { get; set; }

    public void Validar()
    {
        if (string.IsNullOrWhiteSpace(Titulo))
            throw new ArgumentException("El título es obligatorio", nameof(Titulo));

        if (Titulo.Length > 255)
            throw new ArgumentException("El título no puede exceder 255 caracteres", nameof(Titulo));

        if (string.IsNullOrWhiteSpace(Autor))
            throw new ArgumentException("El autor es obligatorio", nameof(Autor));

        if (Autor.Length > 255)
            throw new ArgumentException("El autor no puede exceder 255 caracteres", nameof(Autor));

        if (!string.IsNullOrWhiteSpace(Isbn) && Isbn.Length > 13)
            throw new ArgumentException("El ISBN no puede exceder 13 caracteres", nameof(Isbn));

        if (AnioPublicacion.HasValue && (AnioPublicacion < 1000 || AnioPublicacion > 9999))
            throw new ArgumentException("El año de publicación debe estar entre 1000 y 9999", nameof(AnioPublicacion));

        if (!string.IsNullOrWhiteSpace(Editorial) && Editorial.Length > 255)
            throw new ArgumentException("La editorial no puede exceder 255 caracteres", nameof(Editorial));
    }
}
