using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Digitalis.Infrastructure;
using Digitalis.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Raven.Client.Documents.Session;

namespace Digitalis.Features
{
    public class CreateEntry
    {
        public record Command(string[] Tags) : IRequest<string>;

        public class Authorizer : IAuthorizer<Command>
        {
            private readonly ClaimsIdentity _claimsIdentity;

            public Authorizer(IHttpContextAccessor htx)
            {
                _claimsIdentity = htx.HttpContext?.User.Identity as ClaimsIdentity;
            }

            public bool IsAuthorized(Command request)
            {
                return _claimsIdentity.HasClaim("CreateNewEntry", "");
            }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Tags).NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Command, string>
        {
            private readonly IAsyncDocumentSession _session;

            public Handler(IAsyncDocumentSession session, IHttpContextAccessor htx)
            {
                
                _session = session;
            }

            public async Task<string> Handle(Command command, CancellationToken cancellationToken)
            {
                Entry entry = new Entry
                {
                    Tags = command.Tags.ToList()
                };

                await _session.StoreAsync(entry, cancellationToken);
                await _session.SaveChangesAsync(cancellationToken);

                return entry.Id;
            }
        }
    }
}
