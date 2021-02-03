using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Digitalis.Infrastructure.Mediatr
{
    public class AuthPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IEnumerable<IAuth<TRequest>> _authorizers;

        public AuthPipelineBehavior(IEnumerable<IAuth<TRequest>> authorizers)
        {
            _authorizers = authorizers;
        }

        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            if (_authorizers.Any())
            {
                foreach (IAuth<TRequest> authorizer in _authorizers)
                {
                    authorizer.Authenticate(request);
                    authorizer.Authorize(request);
                }
            }

            // Invoke the next handler
            // (can be another pipeline behavior or the request handler)
            return next();
        }
    }
}
