using System.ComponentModel.DataAnnotations;

namespace SubastaYa.Domain.Entities;

public class Subasta
{
    public int Id { get; set; }
    public int VendedorId { get; set; }
    public Usuario Vendedor { get; set; } = null!;
    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string UrlImagen { get; set; } = string.Empty;
    public decimal PrecioBase { get; set; }
    public decimal IncrementoMinimo { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Estado { get; set; } = "PROGRAMADA";

    [ConcurrencyCheck]
    public int Version { get; set; }

    public ICollection<Puja> Pujas { get; set; } = new List<Puja>();

    public decimal OfertaActual() => Pujas.Any() ? Pujas.Max(p => p.Monto) : PrecioBase;

    public bool EsPujaValida(decimal montoOfertado) => montoOfertado >= OfertaActual() + IncrementoMinimo;

    public bool EstaEnVentanaAntiSniping(DateTime ahora)
        => (FechaFin - ahora).TotalSeconds <= 60 && (FechaFin - ahora).TotalSeconds > 0;

    public void ExtenderCierre() => FechaFin = FechaFin.AddMinutes(2);
}