using MediatR;

namespace Digitalis.Infrastructure.Mediatr
{
    public class AuthRequest<TResponse> : IRequest<TResponse>
    {
        public virtual void Authorize()
        {

        }
    }

    public class AuthRequest : IRequest
    {
        public virtual void Authorize()
        {

        }
    }
}
