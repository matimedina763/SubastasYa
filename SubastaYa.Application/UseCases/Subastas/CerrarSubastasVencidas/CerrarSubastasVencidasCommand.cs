namespace SubastaYa.Application.UseCases.Subastas.CerrarSubastasVencidas;

public class CerrarSubastasVencidasCommand
{
    public DateTime Ahora { get; set; }

    public CerrarSubastasVencidasCommand(DateTime ahora)
    {
        Ahora = ahora;
    }
}