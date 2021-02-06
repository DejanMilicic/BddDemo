using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Digitalis.Features;
using Digitalis.Infrastructure.Services;
using MediatR;

namespace Digitalis.Infrastructure.Mediatr
{
    public class AuthPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : AuthRequest<TResponse>
    {
        private readonly IEnumerable<IAuth<TRequest, TResponse>> _authorizers;
        private Authenticator _user;

        public AuthPipelineBehavior(Authenticator user, IEnumerable<IAuth<TRequest, TResponse>> authorizers)
        {
            _authorizers = authorizers;
            _user = user;
        }

        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            if (request is AuthRequest<TResponse> authRequest)
            {
                _user.Authenticate();
                foreach (IAuth<TRequest, TResponse> authorizer in _authorizers)
                {
                    authorizer.Authorize(request);
                }
            }

            // Invoke the next handler 
            // (can be another pipeline behavior or the request handler) 
            return next();
        }
    }
}
