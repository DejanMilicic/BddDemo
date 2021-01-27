using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Digitalis.Infrastructure;
using Digitalis.Models;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.BulkInsert;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Session;

namespace Digitalis.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EntryController : ControllerBase
    {
        private readonly IAsyncDocumentSession _session;
        private readonly IDocumentStore _store;

        public EntryController(IAsyncDocumentSession session, IDocumentStore store)
        {
            _session = session;
            _store = store;
        }

        [HttpPost("/seed")]
        public async Task<string> Seed()
        {
            DetailedDatabaseStatistics stats = _store.Maintenance.Send(new GetDetailedStatisticsOperation());
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

        public class CreateEntryModel
        {
            public string[] Tags { get; set; }
        }

        [HttpPost("/entry")]
        public async Task<string> Post([FromBody] CreateEntryModel e)
        {
            Entry entry = new Entry
            {
                Tags = e.Tags.ToList()
            };

            await _session.StoreAsync(entry);
            await _session.SaveChangesAsync();

            return entry.Id;
        }
    }
}
