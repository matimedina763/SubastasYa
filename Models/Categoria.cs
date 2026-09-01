namespace MiProyectoApi.Models;

public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Url_icono { get; set; }

    public ICollection<Subasta> Subastas { get; set; } = new List<Subasta>();

}