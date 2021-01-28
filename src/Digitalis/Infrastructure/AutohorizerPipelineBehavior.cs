using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Digitalis.Infrastructure
{
    public class AuthorizerPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IEnumerable<IAuthorizer<TRequest>> _authorizers;

        public AuthorizerPipelineBehavior(IEnumerable<IAuthorizer<TRequest>> authorizers)
            => _authorizers = authorizers;

        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
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

            // Invoke the next handler
            // (can be another pipeline behavior or the request handler)
            return next();
        }
    }
}
