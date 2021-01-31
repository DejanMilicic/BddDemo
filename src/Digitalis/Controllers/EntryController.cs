using System.Collections.Generic;
using System.Threading.Tasks;
using Bogus;
using Digitalis.Features;
using Digitalis.Infrastructure;
using Digitalis.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.BulkInsert;
using Raven.Client.Documents.Operations;

namespace Digitalis.Controllers
{
    [ApiController]
    public class EntryController : ControllerBase
    {
        private readonly IDocumentStore _store;
        private readonly IMediator _mediator;

        public EntryController(IDocumentStore store, IMediator mediator)
        {
            _store = store;
            _mediator = mediator;
        }

        [HttpPost("entry/seed")]
        public async Task<string> Seed()
        {
            DetailedDatabaseStatistics stats = await _store.Maintenance.SendAsync(new GetDetailedStatisticsOperation()).ConfigureAwait(false);
            if (stats.CountOfDocuments > 0)
                return "Database is already seeded";

            Faker<Entry> generator = new Faker<Entry>()
                .StrictMode(true)
                .Ignore(e => e.Id)
                .RuleFor(e => e.Tags, f => Helper.GetRandomTags());

            List<Entry> entries = generator.Generate(20);

            await using BulkInsertOperation bulkInsert = _store.BulkInsert();
            foreach (Entry entry in entries)
            {
                await bulkInsert.StoreAsync(entry);
            }

            return "Database was empty, new data seeded";
        }

        [HttpPost("entry")]
        public async Task<string> Post([FromBody] CreateEntry.Command entry) => await _mediator.Send(entry);
    }
}
