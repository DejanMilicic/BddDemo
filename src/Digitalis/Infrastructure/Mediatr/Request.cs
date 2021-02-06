using MediatR;

namespace Digitalis.Infrastructure.Mediatr
{
    public class Request<TResponse> : IRequest<TResponse>
    {
        public virtual void Authorize()
        {

        }
    }
}
