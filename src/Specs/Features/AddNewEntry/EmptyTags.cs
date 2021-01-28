using System.Linq;
using System.Net;
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
    [Trait("Add New Entry", "Empty Tags")]
    public class EmptyTags : Fixture
    {
        private readonly HttpResponseMessage _response;
        private readonly CreateEntry.Command _newEntry;

        public EmptyTags(WebApplicationFactory<Startup> factory) : base(factory)
        {
            var client = this.CreateAuthenticatedClient(new []
                {
                    new Claim("CreateNewEntry", "")
                });

            _newEntry = new CreateEntry.Command(new string[] {});

            _response = client.PostAsync("/entry",
                Serialize(_newEntry)).Result;

            WaitForIndexing(Store);
            WaitForUserToContinueTheTest(Store);
        }

        [Fact(DisplayName = "1. Bad request is returned")]
        public void StatusReturned()
        {
            _response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact(DisplayName = "2. Zero entries is created in the database")]
        public void OneEntryCreated()
        {
            Store.OpenSession().Query<Entry>().Statistics(out QueryStatistics stats).ToList();

            stats.TotalResults.Should().Be(0);
        }
    }
}
