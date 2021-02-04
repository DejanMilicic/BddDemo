using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Newtonsoft.Json;

namespace Digitalis.Infrastructure.Mediatr
{
    public class ValidatorPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidatorPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
            => _validators = validators;

        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            if (_validators == null) return next();
            if (!_validators.Any()) return next();

            // Invoke the validators
            var failures = _validators
                .Select(validator => validator.Validate(request))
                .SelectMany(result => result.Errors)
                .ToArray();

            if (failures.Length > 0)
            {
                var projection = failures.Select(
                    failure => new ValidationFailure(
                        failure.PropertyName,
                        JsonConvert.SerializeObject(new
                        {
                            message = failure.ErrorMessage,
                            code = failure.ErrorCode,
                            state = failure.CustomState
                        })));

                throw new ValidationException(projection);
            }

            // Invoke the next handler
            // (can be another pipeline behavior or the request handler)
            return next();
        }
    }
}
