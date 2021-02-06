using MediatR;

namespace Digitalis.Infrastructure.Mediatr
{
    public class AuthRequest<TResponse> : IRequest<TResponse>
    {
        public virtual void Authorize()
        {

        }
    }
}
