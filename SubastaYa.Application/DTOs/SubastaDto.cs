namespace SubastaYa.Application.DTOs;

public class SubastaDto
{
    public int? Id { get; set; }
    public string? Titulo { get; set; }
    public string? Descripcion { get; set; }
    public string? UrlImagen { get; set; }
    public string? Categoria { get; set; }
    public decimal? PrecioInicial { get; set; }
    public decimal? OfertaActual { get; set; }
    public int CantidadOfertas { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public bool? Activa { get; set; }
    public string? Estado { get; set; }
    public string? LiderNombre { get; set; }
}