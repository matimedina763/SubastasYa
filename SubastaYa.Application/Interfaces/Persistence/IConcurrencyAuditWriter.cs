namespace SubastaYa.Application.Interfaces.Persistence;

public interface IConcurrencyAuditWriter
{
    Task RegistrarAsync(string ruta, string metodo);
}