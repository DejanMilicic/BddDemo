using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Digitalis.Infrastructure;
using Digitalis.Infrastructure.Guards;
using Digitalis.Infrastructure.Mediatr;
using Digitalis.Models;
using Digitalis.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Raven.Client.Documents;

namespace Digitalis.Features
{
    public class CreateUser
    {
        public record Command(string email/*, Dictionary<string, string> Claims*/) : IRequest<string>;

        public class Auth : Auth<Command>
        {
            public Auth(IHttpContextAccessor ctx, IDocumentStore store) : base(ctx, store)
            {
            }

            public override void Authorize(Command request)
            {
                AuthorizationGuard.AffirmClaim(User, AppClaims.CreateUser);
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
                user.Email = command.email;
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
