using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Digitalis.Infrastructure.Services;
using MediatR;

namespace Digitalis.Infrastructure.Mediatr
{
    internal class AuthPipelineQueryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : AuthRequest<TResponse>
    {
        public AuthPipelineQueryBehavior(Authenticator authenticator,
            IEnumerable<IAuth<TRequest, TResponse>> authorizers)
        {
            _ = authenticator.User;
        }

        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            return next();
        }
    }

    internal class AuthPipelineCommandBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : AuthRequest
    {
        public AuthPipelineCommandBehavior(Authenticator authenticator,
            IEnumerable<IAuth<TRequest>> authorizers)
        {
            _ = authenticator.User;
        }

        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            return next();
        }
    }
}
