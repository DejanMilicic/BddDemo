using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Digitalis.Infrastructure;
using Digitalis.Infrastructure.Guards;
using Digitalis.Infrastructure.Mediatr;
using Digitalis.Infrastructure.Services;
using Digitalis.Models;
using Digitalis.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Raven.Client.Documents;

namespace Digitalis.Features
{
    public class CreateUser
    {
        public class Command : Request<string>
        {
            public string Email { get; set; }
            public Dictionary<string, string> Claims { get; set; }
        }

        public class Auth : IAuth<Command, string>
        {
            private User _user;

            public Auth(CurrentUser user)
            {
                user.Authenticate();
                _user = user.User;
            }

            public void Authorize(Command request)
            {
                AuthorizationGuard.AffirmClaim(_user, AppClaims.CreateUser);
            }
        }

        public class Handler : IRequestHandler<Command, string>
        {
            private readonly IDocumentStore _store;
            private readonly IMailer _mailer;

            public Handler(IDocumentStore store, IMailer mailer)
            {
                _store = store;
                _mailer = mailer;
            }

            public async Task<string> Handle(Command command, CancellationToken cancellationToken)
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

                return user.Id;
            }
        }
    }
}
