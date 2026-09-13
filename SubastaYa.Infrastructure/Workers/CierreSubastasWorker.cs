using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SubastaYa.Application.UseCases.Subastas.CerrarSubastasVencidas;

namespace SubastaYa.Infrastructure.Workers;

public class CierreSubastasWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public CierreSubastasWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var handler = scope.ServiceProvider.GetRequiredService<CerrarSubastasVencidasCommandHandler>();
                var command = new CerrarSubastasVencidasCommand(DateTime.UtcNow);

                await handler.Handle(command);
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}