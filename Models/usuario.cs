namespace MiProyectoApi.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Email { get; set;}
    public string Nombre { get; set;}
    public string PasswordHash {get; set;}
    public DateTime FechaRegistro {get; set;} = DateTime.UtcNow;

    public ICollection<Subasta> Subastas { get; set;} = new List<Subasta>();
    public ICollection<Puja> Pujas {get; set;} = new List<Puja>();

    public Billetera Billetera { get; set;}
}