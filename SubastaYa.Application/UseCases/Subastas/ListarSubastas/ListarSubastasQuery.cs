namespace SubastaYa.Application.UseCases.Subastas.ListarSubastas;

public class ListarSubastasQuery
{
    public string? Estado { get; set; }
    public int? CategoriaId { get; set; }
    public decimal? PrecioMin { get; set; }
    public decimal? PrecioMax { get; set; }
    public string? OrdenarPor { get; set; } // "MenorTiempoRestante" o "MayorPuja"
}