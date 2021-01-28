using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Digitalis;
using Digitalis.Features;
using Digitalis.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Specs.Infrastructure;
using Xunit;

namespace Specs.Controllers
{
    public class EntryControllerTests : Fixture
    {
        public EntryControllerTests(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Fact]
        public async Task EntryPost_CreatesOneEntry_WithExpectedContent()
        {
            var httpClient = this.CreateAuthenticatedClient(new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, "12345678-1234-1234-1234-123456789012"),
                    new Claim(ClaimTypes.Name, "TestUser"),
                    new Claim(ClaimTypes.Email, "test.user@example.com"),
                    new Claim("CreateNewEntry", "")
                });

            var newEntry = new CreateEntry.Command(new[] { "tag1", "tag2", "tag3" });

            var result = await httpClient.PostAsync("/entry",
                Serialize(newEntry));

            result.StatusCode.Should().Be(200);

            var session = Store.OpenSession();

            WaitForIndexing(Store);
            WaitForUserToContinueTheTest(Store);

            List<Entry> entries = session.Query<Entry>().ToList();

            entries.Count.Should().Be(1);

            var entry = entries.Single();
            entry.Tags.Should().BeEquivalentTo(newEntry.Tags);
        }

        [Fact(DisplayName = "Unauthorized user adds entry")]
        public async Task UnauthorizedUserAddsEntry()
        {
            var httpClient = this.CreateAuthenticatedClient(
                new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, "12345678-1234-1234-1234-123456789012"),
                    new Claim(ClaimTypes.Name, "TestUser"),
                    new Claim(ClaimTypes.Email, "test.user@example.com"),
                });

            var result = await httpClient.PostAsync("/entry",
                Serialize(
                    new CreateEntry.Command(new[] { "tag1", "tag2", "tag3" })));


            result.StatusCode.Should().Be(403);
        }

        [Fact(DisplayName = "Unauthenticated user adds entry")]
        public async Task UnauthenticatedUserAddsEntry()
        {
            var result = await this.CreateAnonymousClient().PostAsync("/entry",
                Serialize(
                    new CreateEntry.Command(new[] { "tag1", "tag2", "tag3" })));

            result.StatusCode.Should().Be(401);
        }
    }
}
