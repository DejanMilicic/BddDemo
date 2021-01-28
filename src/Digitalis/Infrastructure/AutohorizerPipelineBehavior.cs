using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Digitalis.Infrastructure
{
    public class AuthorizerPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IEnumerable<IAuthorizer<TRequest>> _authorizers;
        private readonly IHttpContextAccessor _ctx;

        public AuthorizerPipelineBehavior(IEnumerable<IAuthorizer<TRequest>> authorizers, IHttpContextAccessor ctx)
        {
            _authorizers = authorizers;
            _ctx = ctx;
        }

        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            if (_authorizers.Any())
            {
                // check if user is authenticated
                if (_ctx.HttpContext?.User?.Identity == null)
                    throw new AuthenticationException();

                if ((bool)!_ctx.HttpContext?.User.Identity.IsAuthenticated)
                    throw new AuthenticationException();

                // Invoke the authorizers
                var authorizations = _authorizers
                    .Select(authorizer => authorizer.IsAuthorized(request))
                    .ToArray();

                if (authorizations.Any(x => x == false))
                {
                    // throw an error,
                    // this stops the execution of the request
                    throw new UnauthorizedAccessException();
                }
            }

            // Invoke the next handler
            // (can be another pipeline behavior or the request handler)
            return next();
        }
    }
}
