using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Digitalis.Infrastructure;
using Digitalis.Infrastructure.Mediatr;
using Digitalis.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Raven.Client.Documents.Session;

namespace Digitalis.Features
{
    public class FetchEntry
    {
        public record Query(string id) : IRequest<Entry>;

        public class Authorizer : IAuthorizer<Query>
        {
            private readonly ClaimsIdentity _claimsIdentity;

            public Authorizer(IHttpContextAccessor htx)
            {
                _claimsIdentity = htx.HttpContext?.User.Identity as ClaimsIdentity;
            }

            public bool IsAuthorized(Query request)
            {
                return _claimsIdentity.HasClaim(DigitalisClaims.FetchEntry, "");
            }
        }

        public class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(x => x.id).NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Query, Entry>
        {
            private readonly IAsyncDocumentSession _session;

            public Handler(IAsyncDocumentSession session)
            {
                _session = session;
            }

            public async Task<Entry> Handle(Query query, CancellationToken cancellationToken)
            {
                Entry entry = await _session.LoadAsync<Entry>(query.id, cancellationToken);

                if (entry == null)
                    throw new KeyNotFoundException();

                return entry;
            }
        }
    }
}
