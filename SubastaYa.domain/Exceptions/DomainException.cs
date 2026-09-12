namespace SubastaYa.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class SubastaNoEncontradaException : DomainException
{
    public SubastaNoEncontradaException(int id) : base($"No se encontró la subasta con id {id}.") { }
}

public class PujaInvalidaException : DomainException
{
    public PujaInvalidaException(string mensaje) : base(mensaje) { }
}

public class FondosInsuficientesException : DomainException
{
    public FondosInsuficientesException() : base("Fondos insuficientes para realizar la puja.") { }
}

public class SubastaNoActivaException : DomainException
{
    public SubastaNoActivaException() : base("La subasta no está activa, no se pueden registrar pujas.") { }
}