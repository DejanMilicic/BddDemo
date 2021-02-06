﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Digitalis.Infrastructure;
using Digitalis.Infrastructure.Guards;
using Digitalis.Infrastructure.Mediatr;
using Digitalis.Infrastructure.Services;
using Digitalis.Models;
using FluentValidation;
using MediatR;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace Digitalis.Features
{
    public class FetchEntry
    {
        public class Query : AuthRequest<Entry>
        {
            public string Id { get; set; }
        }

        public class Auth : IAuth<Query, Entry>
        {
            public Auth(Authenticator authenticator)
            {
                var user = authenticator.User;
                AuthorizationGuard.AffirmClaim(user, AppClaims.FetchEntry);
            }
        }

        public class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();
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
                using var session = _store.OpenAsyncSession(new SessionOptions{ NoTracking = true });
                Entry entry = await session.LoadAsync<Entry>(query.Id, cancellationToken);

                if (entry == null)
                    throw new KeyNotFoundException();

                return entry;
            }
        }
    }
}
