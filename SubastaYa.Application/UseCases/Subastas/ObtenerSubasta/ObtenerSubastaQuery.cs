using MediatR; 
using SubastaYa.Application.DTOs;  

namespace SubastaYa.Application.UseCases.Subastas.ObtenerSubasta;
public class ObtenerSubastaQuery : IRequest<SubastaDto>   
{
    public Guid Id { get; set; }
    public ObtenerSubastaQuery(Guid id)
    {
        Id = id;
    }
        
}
    





