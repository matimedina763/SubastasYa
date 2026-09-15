namespace SubastaYa.Application.UseCases.Subastas.CrearSubasta;

public class CrearSubastaCommand
{
    public int VendedorId { get; set; }
    public string Titulo { get; set; } 
    public string Descripcion { get; set; } 
    public string UrlImagen { get; set; } 
    public int CategoriaId { get; set; } 
    public decimal PrecioBase { get; set; }
    public decimal IncrementoMinimo { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    

    public CrearSubastaCommand(string titulo, string descripcion, string urlImagen, int categoriaId, decimal precioBase, decimal incrementoMinimo, DateTime fechaInicio, DateTime fechaFin)
    {
        Titulo = titulo;
        Descripcion = descripcion;
        UrlImagen = urlImagen;
        CategoriaId = categoriaId;
        PrecioBase = precioBase;
        IncrementoMinimo = incrementoMinimo;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
    }
}