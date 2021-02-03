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
using Raven.Client.Documents.Session;

namespace Digitalis.Features
{
    public class CreateUser
    {
        public record Command(string email/*, Dictionary<string, string> Claims*/) : IRequest<string>;

        public class Auth : Auth<Command>
        {
            public Auth(IHttpContextAccessor ctx, IDocumentSession session) : base(ctx, session)
            {
            }

            public override void Authorize(Command request)
            {
                AuthorizationGuard.AffirmClaim(User, AppClaims.CreateUser);
            }
        }

        public class Handler : IRequestHandler<Command, string>
        {
            private readonly IAsyncDocumentSession _session;
            private readonly IMailer _mailer;

            public Handler(IAsyncDocumentSession session, IMailer mailer)
            {
                _session = session;
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

                await _session.StoreAsync(user, cancellationToken);
                await _session.SaveChangesAsync(cancellationToken);

                _mailer.SendMail("admin@site.com", "New user created", "Email body...");

                return user.Id;
            }
        }
    }
}
