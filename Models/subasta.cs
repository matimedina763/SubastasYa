using System.ComponentModel.DataAnnotations;

namespace MiProyectoApi.Models;

public class Subasta
{
    public int Id  { get; set; }

    public int VendedorId { get; set; }
    public Usuario Vendedor { get; set; }

    public int CategoriaId { get; set; }
    public Categoria Categoria {get; set;}

    public string Titulo { get; set; }
    public string Descripcion { get; set; }
    public string Url_imagen {get; set;}
    public decimal precio_base {get; set;}
    public decimal incremento_minimo {get; set;}
    public DateTime fecha_inicio {get; set;}
    public DateTime fecha_fin {get; set;}
    public string estado {get; set;} = "PROGRAMADA";

    [Timestamp]
    public byte[] Version { get; set; }
    public ICollection<Puja> Pujas { get; set; } = new List<Puja>();
    
}