using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Digitalis.Infrastructure.Mediatr;
using Digitalis.Models;
using FluentValidation;
using MediatR;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace Digitalis.Features
{
    public class AnonFetchEntry
    {
        public class Query : AnonRequest<Entry>
        {
            public string Id { get; set; }
        }

        internal class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();
            }
        }

        internal class Handler : IRequestHandler<Query, Entry>
        {
            private readonly IDocumentStore _store;

            public Handler(IDocumentStore store)
            {
                _store = store;
            }

            public async Task<Entry> Handle(Query query, CancellationToken cancellationToken)
            {
                using var session = _store.OpenAsyncSession(new SessionOptions { NoTracking = true });
                Entry entry = await session.LoadAsync<Entry>(query.Id, cancellationToken);

                if (entry == null)
                    throw new KeyNotFoundException();

                return entry;
            }
        }
    }
}
