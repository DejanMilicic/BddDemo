using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Digitalis.Infrastructure;
using Digitalis.Infrastructure.Guards;
using Digitalis.Infrastructure.Mediatr;
using Digitalis.Infrastructure.Services;
using Digitalis.Models;
using Digitalis.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Raven.Client.Documents;

namespace Digitalis.Features
{
    public class CreateEntry
    {
        public class Command : AuthRequest<string>
        {
            public string[] Tags { get; set; }
        }

        internal class Auth : IAuth<Command, string>
        {
            public Auth(Authenticator authenticator)
            {
                var user = authenticator.User;
                AuthorizationGuard.AffirmClaim(user, AppClaims.CreateNewEntry);
            }
        }

        internal class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Tags).NotEmpty();
            }
        }

        internal class Handler : IRequestHandler<Command, string>
        {
            private readonly IDocumentStore _store;
            private readonly IMailer _mailer;
            private User _user;

            public Handler(IDocumentStore store, IMailer mailer, Authenticator auth)
            {
                _store = store;
                _mailer = mailer;
                _user = auth.User;
            }

            public async Task<string> Handle(Command command, CancellationToken cancellationToken)
            {
                using var session = _store.OpenAsyncSession();

                Entry entry = new Entry
                {
                    Tags = command.Tags.ToList()
                };

                await session.StoreAsync(entry, cancellationToken);
                await session.SaveChangesAsync(cancellationToken);

                _mailer.SendMail("admin@site.com", "New entry created", String.Join(", ", entry.Tags));

                return entry.Id;
            }
        }
    }
}
