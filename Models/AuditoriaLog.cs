namespace MiProyectoApi.Models;

public class AuditoriaLog
{
    public int Id { get; set; }
    public string Entidad { get; set; }
    public int EntidadId { get; set; }
    public string Accion { get; set; }
    public int? UsuarioId { get; set; }

    public string DetalleJson { get; set; }

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    
}