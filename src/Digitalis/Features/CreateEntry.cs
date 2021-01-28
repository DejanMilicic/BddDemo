using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Digitalis.Models;
using MediatR;
using Raven.Client.Documents.Session;

namespace Digitalis.Features
{
    public class CreateEntry
    {
        public record Command(string[] Tags) : IRequest<string>;

        public class Handler : IRequestHandler<Command, string>
        {
            private readonly IAsyncDocumentSession _session;

            public Handler(IAsyncDocumentSession session)
            {
                _session = session;
            }

            public async Task<string> Handle(Command command, CancellationToken cancellationToken)
            {
                Entry entry = new Entry
                {
                    Tags = command.Tags.ToList()
                };

                await _session.StoreAsync(entry, cancellationToken);
                await _session.SaveChangesAsync(cancellationToken);

                return entry.Id;
            }
        }
    }
}
