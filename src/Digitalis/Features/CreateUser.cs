using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Digitalis.Infrastructure;
using Digitalis.Infrastructure.Guards;
using Digitalis.Infrastructure.Mediatr;
using Digitalis.Infrastructure.Services;
using Digitalis.Models;
using Digitalis.Services;
using MediatR;
using Raven.Client.Documents;

namespace Digitalis.Features
{
    public class CreateUser
    {
        public class Command : AuthRequest
        {
            public string Email { get; set; }

            public Dictionary<string, string> Claims { get; set; }
        }

        internal class Auth : IAuth<Command>
        {
            public Auth(Authenticator authenticator)
            {
                var user = authenticator.User;
                AuthorizationGuard.AffirmClaim(user, AppClaims.CreateUser);
            }
        }

        internal class Handler : AsyncRequestHandler<Command>
        {
            private readonly IDocumentStore _store;
            private readonly IMailer _mailer;

            public Handler(IDocumentStore store, IMailer mailer)
            {
                _store = store;
                _mailer = mailer;
            }

            protected override async Task Handle(Command command, CancellationToken cancellationToken)
            {
                User user = new User();
                user.Email = command.Email;
                user.Claims = new List<(string, string)>
                    {
                        (AppClaims.CreateNewEntry, "")
                    };

                using var session = _store.OpenAsyncSession();
                await session.StoreAsync(user, cancellationToken);
                await session.SaveChangesAsync(cancellationToken);

                _mailer.SendMail("admin@site.com", "New user created", "Email body...");
            }
        }
    }
}
