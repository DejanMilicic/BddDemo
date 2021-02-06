using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Digitalis.Infrastructure;
using Digitalis.Infrastructure.Guards;
using Digitalis.Infrastructure.Mediatr;
using Digitalis.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Raven.Client.Documents;

namespace Digitalis.Features
{
    public class FetchEntry
    {
        public record Query(string id) : IRequest<Entry>;

        public class Auth : Auth<Query>
        {
            public Auth(IHttpContextAccessor ctx, IDocumentStore store) : base(ctx, store)
            {
            }

            public override void Authorize(Query request)
            {
                AuthorizationGuard.AffirmClaim(User, AppClaims.FetchEntry);
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
            private readonly IDocumentStore _store;

            public Handler(IDocumentStore store)
            {
                _store = store;
            }

            public async Task<Entry> Handle(Query query, CancellationToken cancellationToken)
            {
                using var session = _store.OpenAsyncSession();
                Entry entry = await session.LoadAsync<Entry>(query.id, cancellationToken);

                if (entry == null)
                    throw new KeyNotFoundException();

                return entry;
            }
        }
    }
}
