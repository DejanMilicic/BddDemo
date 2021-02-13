using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Hellang.Middleware.ProblemDetails;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Digitalis.Infrastructure.Mediatr
{
    internal class ValidatorPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
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
                    failure => new {
                        Name = failure.PropertyName,
                        Message = failure.ErrorMessage
                        });

                var problemDetails = new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "One or more validation errors occurred.",
                    Status = 400,
                };

                problemDetails.Extensions.Add("errors", projection);

                throw new ProblemDetailsException(problemDetails);
            }

            // Invoke the next handler
            // (can be another pipeline behavior or the request handler)
            return next();
        }

        private static string BuildErrorMessage(IEnumerable<ValidationFailure> errors)
        {
            var arr = errors.Select(x => $"{x.PropertyName}: {x.ErrorMessage}");
            return string.Join(string.Empty, arr);
        }
    }
}
