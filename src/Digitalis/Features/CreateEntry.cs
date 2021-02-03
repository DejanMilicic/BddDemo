using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Digitalis.Infrastructure;
using Digitalis.Infrastructure.Guards;
using Digitalis.Infrastructure.Mediatr;
using Digitalis.Models;
using Digitalis.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Raven.Client.Documents.Session;

namespace Digitalis.Features
{
    public class CreateEntry
    {
        public record Command(string[] Tags) : IRequest<string>;

        public class Auth : Auth<Command>
        {
            public Auth(IHttpContextAccessor ctx, IDocumentSession session) : base(ctx, session)
            {
            }

            public override void Authorize(Command request)
            {
                AuthorizationGuard.AffirmClaim(User, AppClaims.CreateNewEntry);
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
            private readonly IMailer _mailer;

            public Handler(IAsyncDocumentSession session, IMailer mailer)
            {
                _session = session;
                _mailer = mailer;
            }

            public async Task<string> Handle(Command command, CancellationToken cancellationToken)
            {
                Entry entry = new Entry
                {
                    Tags = command.Tags.ToList()
                };

                await _session.StoreAsync(entry, cancellationToken);
                await _session.SaveChangesAsync(cancellationToken);

                _mailer.SendMail("admin@site.com", "New entry created", String.Join(", ", entry.Tags));

                return entry.Id;
            }
        }
    }
}
