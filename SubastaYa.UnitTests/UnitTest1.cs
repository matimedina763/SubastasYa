using NSubstitute;
using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Application.UseCases.Subastas.RegistrarPuja;
using SubastaYa.Domain.Entities;
using SubastaYa.Domain.Exceptions;
using Xunit;

namespace SubastaYa.UnitTests;

public class RegistrarPujaCommandHandlerTests
{
    private static Subasta CrearSubastaBase(string estado = "ACTIVA", int vendedorId = 1, List<Puja>? pujas = null)
    {
        return new Subasta
        {
            Id = 1,
            VendedorId = vendedorId,
            Estado = estado,
            PrecioBase = 1000,
            IncrementoMinimo = 100,
            FechaFin = DateTime.UtcNow.AddHours(1),
            Pujas = pujas ?? new List<Puja>()
        };
    }

    private static Billetera CrearBilleteraBase(int usuarioId = 2, decimal saldoTotal = 5000, decimal saldoRetenido = 0)
    {
        return new Billetera { Id = 1, UsuarioId = usuarioId, SaldoTotal = saldoTotal, SaldoRetenido = saldoRetenido };
    }

    [Fact]
    public async Task Handle_PujaValida_RegistraCorrectamente()
    {
        var subastaRepo = Substitute.For<ISubastaRepository>();
        var billeteraRepo = Substitute.For<IBilleteraRepository>();
        var auditoriaLogRepo = Substitute.For<IAuditoriaLogRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        unitOfWork.ExecuteInTransactionAsync<Puja>(Arg.Any<Func<Task<Puja>>>())
            .Returns(callInfo => callInfo.Arg<Func<Task<Puja>>>()());

        var subasta = CrearSubastaBase();
        var billetera = CrearBilleteraBase();

        subastaRepo.ObtenerSubastaPorIdAsync(1).Returns(subasta);
        billeteraRepo.ObtenerPorUsuarioIdAsync(2).Returns(billetera);

        var handler = new RegistrarPujaCommandHandler(subastaRepo, billeteraRepo, unitOfWork, auditoriaLogRepo);
        var comando = new RegistrarPujaCommand(1, 2, 1100);

        await handler.Handle(comando);

        subastaRepo.Received(1).AgregarPuja(Arg.Any<Puja>());
    }

    [Fact]
    public async Task Handle_MontoMenorAlMinimo_LanzaPujaInvalidaException()
    {
        var subastaRepo = Substitute.For<ISubastaRepository>();
        var billeteraRepo = Substitute.For<IBilleteraRepository>();
        var auditoriaLogRepo = Substitute.For<IAuditoriaLogRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        subastaRepo.ObtenerSubastaPorIdAsync(1).Returns(CrearSubastaBase());

        var handler = new RegistrarPujaCommandHandler(subastaRepo, billeteraRepo, unitOfWork, auditoriaLogRepo);
        var comando = new RegistrarPujaCommand(1, 2, 50);

        await Assert.ThrowsAsync<PujaInvalidaException>(() => handler.Handle(comando));
    }

    [Fact]
    public async Task Handle_SubastaNoActiva_LanzaSubastaNoActivaException()
    {
        var subastaRepo = Substitute.For<ISubastaRepository>();
        var billeteraRepo = Substitute.For<IBilleteraRepository>();
        var auditoriaLogRepo = Substitute.For<IAuditoriaLogRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        subastaRepo.ObtenerSubastaPorIdAsync(1).Returns(CrearSubastaBase(estado: "PROGRAMADA"));

        var handler = new RegistrarPujaCommandHandler(subastaRepo, billeteraRepo, unitOfWork, auditoriaLogRepo);
        var comando = new RegistrarPujaCommand(1, 2, 1100);

        await Assert.ThrowsAsync<SubastaNoActivaException>(() => handler.Handle(comando));
    }

    [Fact]
    public async Task Handle_FondosInsuficientes_LanzaFondosInsuficientesException()
    {
        var subastaRepo = Substitute.For<ISubastaRepository>();
        var billeteraRepo = Substitute.For<IBilleteraRepository>();
        var auditoriaLogRepo = Substitute.For<IAuditoriaLogRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        subastaRepo.ObtenerSubastaPorIdAsync(1).Returns(CrearSubastaBase());
        billeteraRepo.ObtenerPorUsuarioIdAsync(2).Returns(CrearBilleteraBase(saldoTotal: 500));

        var handler = new RegistrarPujaCommandHandler(subastaRepo, billeteraRepo, unitOfWork, auditoriaLogRepo);
        var comando = new RegistrarPujaCommand(1, 2, 1100);

        await Assert.ThrowsAsync<FondosInsuficientesException>(() => handler.Handle(comando));
    }

    [Fact]
    public async Task Handle_VendedorPujaEnSuPropiaSubasta_LanzaPujaInvalidaException()
    {
        var subastaRepo = Substitute.For<ISubastaRepository>();
        var billeteraRepo = Substitute.For<IBilleteraRepository>();
        var auditoriaLogRepo = Substitute.For<IAuditoriaLogRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        subastaRepo.ObtenerSubastaPorIdAsync(1).Returns(CrearSubastaBase(vendedorId: 2));

        var handler = new RegistrarPujaCommandHandler(subastaRepo, billeteraRepo, unitOfWork, auditoriaLogRepo);
        var comando = new RegistrarPujaCommand(1, 2, 1100);

        await Assert.ThrowsAsync<PujaInvalidaException>(() => handler.Handle(comando));
    }

    [Fact]
    public async Task Handle_PujaDentroDeVentanaAntiSniping_ExtiendeElCierre()
    {
        var subastaRepo = Substitute.For<ISubastaRepository>();
        var billeteraRepo = Substitute.For<IBilleteraRepository>();
        var auditoriaLogRepo = Substitute.For<IAuditoriaLogRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        unitOfWork.ExecuteInTransactionAsync<Puja>(Arg.Any<Func<Task<Puja>>>())
            .Returns(callInfo => callInfo.Arg<Func<Task<Puja>>>()());

        var subasta = CrearSubastaBase();
        subasta.FechaFin = DateTime.UtcNow.AddSeconds(30);
        var fechaFinOriginal = subasta.FechaFin;

        subastaRepo.ObtenerSubastaPorIdAsync(1).Returns(subasta);
        billeteraRepo.ObtenerPorUsuarioIdAsync(2).Returns(CrearBilleteraBase());

        var handler = new RegistrarPujaCommandHandler(subastaRepo, billeteraRepo, unitOfWork, auditoriaLogRepo);
        var comando = new RegistrarPujaCommand(1, 2, 1100);

        await handler.Handle(comando);

        Assert.True(subasta.FechaFin > fechaFinOriginal);
    }
}