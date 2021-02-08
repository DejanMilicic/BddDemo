using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Digitalis.Infrastructure.Services;
using MediatR;

namespace Digitalis.Infrastructure.Mediatr
{
    public class AuthPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : AuthRequest<TResponse>
    {
        public AuthPipelineBehavior(Authenticator authenticator, IEnumerable<IAuth<TRequest, TResponse>> authorizers)
        {
            _ = authenticator.User;
        }

        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            return next();
        }
    }
}
