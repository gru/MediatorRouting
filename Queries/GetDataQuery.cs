using MediatR;
using Microsoft.AspNetCore.Components;

namespace MediatorRouting.Queries
{
    public record GetDataQuery(string Name)
        : IRequest<Data>;

    public record Data(string Value);
    
    [Route("api/test")]
    public class GetDataQueryHandler : RequestHandler<GetDataQuery, Data> 
    {
        protected override Data Handle(GetDataQuery request)
        {
            return new Data($"Test {request.Name}");
        }
    }
}