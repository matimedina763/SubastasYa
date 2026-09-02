namespace MiProyectoAPI.Models;

public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Url_icono { get; set; } = string.Empty;

    public ICollection<Subasta> Subastas { get; set; } = new List<Subasta>();

}