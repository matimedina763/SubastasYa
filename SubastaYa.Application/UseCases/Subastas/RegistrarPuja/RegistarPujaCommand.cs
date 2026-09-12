namespace SubastaYa.Application.UseCases.Subastas.RegistrarPuja;

public class RegistrarPujaCommand
{
    public int SubastaId { get; set; }
    public int CompradorId { get; set; }
    public decimal Monto { get; set; }

    public RegistrarPujaCommand(int subastaId, int compradorId, decimal monto)
    {
        SubastaId = subastaId;
        CompradorId = compradorId;
        Monto = monto;
    }
}