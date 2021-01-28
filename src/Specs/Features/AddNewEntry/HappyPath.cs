using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using Digitalis;
using Digitalis.Features;
using Digitalis.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Raven.Client.Documents.Session;
using Specs.Infrastructure;
using Xunit;

namespace Specs.Features.AddNewEntry
{
    [Trait("Add New Entry", "Happy Path")]
    public class HappyPath : Fixture
    {
        private readonly HttpResponseMessage _response;
        private readonly CreateEntry.Command _newEntry;

        public HappyPath(WebApplicationFactory<Startup> factory) : base(factory)
        {
            var client = this.CreateAuthenticatedClient(new []
                {
                    new Claim("CreateNewEntry", "")
                });

            _newEntry = new CreateEntry.Command(new[] { "tag1", "tag2", "tag3" });

            _response = client.PostAsync("/entry",
                Serialize(_newEntry)).Result;

            var session = Store.OpenSession();

            WaitForIndexing(Store);
            WaitForUserToContinueTheTest(Store);
        }

        [Fact(DisplayName = "1. Status 200 is returned")]
        public void StatusReturned()
        {
            _response.StatusCode.Should().Be(200);
        }

        [Fact(DisplayName = "2. One entry is created")]
        public void OneEntryCreated()
        {
            Store.OpenSession().Query<Entry>().Statistics(out QueryStatistics stats);

            stats.TotalResults.Should().Be(0);
        }

        [Fact(DisplayName = "3. New entry has expected content")]
        public void ExpectedContent()
        {
            var entry = Store.OpenSession().Query<Entry>().Single();

            entry.Tags.Should().BeEquivalentTo(_newEntry.Tags);
        }
    }
}
