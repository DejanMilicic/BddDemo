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
    [Trait("Add New Entry", "Unauthorized User")]
    public class UnauthorizedUser : Fixture
    {
        private readonly HttpResponseMessage _response;
        private readonly CreateEntry.Command _newEntry;

        public UnauthorizedUser(WebApplicationFactory<Startup> factory) : base(factory)
        {
            var client = this.CreateAuthenticatedClient(new Claim[] { });

            _newEntry = new CreateEntry.Command(new[] { "tag1", "tag2", "tag3" });

            _response = client.PostAsync("/entry",
                Serialize(_newEntry)).Result;

            WaitForIndexing(Store);
            WaitForUserToContinueTheTest(Store);
        }

        [Fact(DisplayName = "1. Status 403 is returned")]
        public void StatusReturned()
        {
            _response.StatusCode.Should().Be(403);
        }

        [Fact(DisplayName = "2. Entry is not added to the database")]
        public void EntryNotAdded()
        {
            Store.OpenSession().Query<Entry>().Statistics(out QueryStatistics stats).ToList();

            stats.TotalResults.Should().Be(0);
        }
    }
}
