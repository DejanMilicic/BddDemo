using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
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
using Raven.Client.Documents.Session;

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

        /// <summary>
        /// Creates new entry
        /// </summary>
        /// <remarks>
        /// Entries will be expanded in next versions of this API
        /// </remarks>
        /// <returns>ID of new entry that was created</returns>
        /// <param name="entry">Entry with tags</param>
        /// <response code="200">Entry created</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">User is not authorized</response>
        /// <response code="500">Server error occurred</response>
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        [Produces(typeof(string))]
        [HttpPost("entry")]
        public async Task<string> Post([FromBody] CreateEntry.Command entry) => await _mediator.Send(entry);
    }
}
