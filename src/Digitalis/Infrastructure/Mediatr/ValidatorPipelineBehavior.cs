using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

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
                // Map the validation failures and throw an error,
                // this stops the execution of the request
                var errors = failures
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(k => k.Key, v => v.Select(x => x.ErrorMessage).ToArray());
                throw new InputValidationException(errors);
            }

            // Invoke the next handler
            // (can be another pipeline behavior or the request handler)
            return next();
        }
    }

    [Serializable]
    internal class InputValidationException : Exception
    {
        private Dictionary<string, string[]> errors;

        public InputValidationException()
        {
        }

        public InputValidationException(Dictionary<string, string[]> errors)
        {
            this.errors = errors;
        }

        public InputValidationException(string message) : base(message)
        {
        }

        public InputValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InputValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
