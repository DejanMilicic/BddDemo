using MediatR;

namespace Digitalis.Infrastructure
{
    public class AnonymousRequest<TResponse> : IRequest<TResponse>
    {
    }
}
