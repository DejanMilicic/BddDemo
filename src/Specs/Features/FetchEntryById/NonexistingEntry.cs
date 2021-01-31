using System.Net;
using System.Net.Http;
using System.Security.Claims;
using Digitalis;
using Digitalis.Features;
using Digitalis.Infrastructure;
using Digitalis.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Specs.Infrastructure;
using Xunit;

namespace Specs.Features.FetchEntryById
{
    [Trait("Fetch entry", "Nonexisting entry")]
    public class NonexistingEntry : Fixture
    {
        private readonly HttpResponseMessage _response;
        private readonly Entry _fetchedEntry;

        public NonexistingEntry(WebApplicationFactory<Startup> factory) : base(factory)
        {
            var readerClient = this.CreateAuthenticatedClient(new []
                {
                    new Claim(DigitalisClaims.FetchEntry, "")
                });

            _response = readerClient.GetAsync($"/entry?id=NONEXISTING").Result;

            _fetchedEntry = Deserialize<Entry>(_response);

            WaitForIndexing(Store);
            WaitForUserToContinueTheTest(Store);
        }

        [Fact(DisplayName = "1. Not found is returned")]
        public void NotFound()
        {
            _response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "2. Entry is not returned")]
        public void EntryNotReturned()
        {
            _fetchedEntry.Should().BeNull();
        }
    }
}
