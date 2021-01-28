using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Digitalis;
using Digitalis.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Specs.Infrastructure;
using Xunit;

namespace Specs.Controllers
{
    public class EntryControllerTests : Fixture
    {
        public EntryControllerTests(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        private class CreateEntryModel
        {
            public CreateEntryModel(string[] tags)
            {
                Tags = tags;
            }

            public string[] Tags { get; }
        }

        [Fact]
        public async Task EntryPost_CreatesOneEntry_WithExpectedContent()
        {
            var httpClient = this.CreateAuthenticatedClient(
                new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, "12345678-1234-1234-1234-123456789012"),
                    new Claim(ClaimTypes.Name, "TestUser"),
                    new Claim(ClaimTypes.Email, "test.user@example.com"),
                    new Claim("CreateNewEntry", "")
                });

            var newEntryModel = new CreateEntryModel(new[] { "tag1", "tag2", "tag3" });

            var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(JsonSerializer.Serialize(newEntryModel), Encoding.UTF8, MediaTypeNames.Application.Json),
                RequestUri = new Uri(httpClient.BaseAddress + "entry")
            };

            var result = await httpClient.SendAsync(requestMessage);

            result.StatusCode.Should().Be(200);

            var session = Store.OpenSession();

            WaitForIndexing(Store);
            WaitForUserToContinueTheTest(Store);

            List<Entry> entries = session.Query<Entry>().ToList();

            entries.Count.Should().Be(1);

            var entry = entries.Single();
            entry.Tags.Should().BeEquivalentTo(newEntryModel.Tags);
        }

        [Fact(DisplayName = "Unauthorized user adds entry")]
        public async void UnauthorizedUserAddsEntry()
        {
            var httpClient = this.CreateAuthenticatedClient(
                new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, "12345678-1234-1234-1234-123456789012"),
                    new Claim(ClaimTypes.Name, "TestUser"),
                    new Claim(ClaimTypes.Email, "test.user@example.com"),
                });

            var newEntryModel = new CreateEntryModel(new[] { "tag1", "tag2", "tag3" });

            var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(JsonSerializer.Serialize(newEntryModel), Encoding.UTF8, MediaTypeNames.Application.Json),
                RequestUri = new Uri(httpClient.BaseAddress + "entry")
            };

            var result = await httpClient.SendAsync(requestMessage);

            result.StatusCode.Should().Be(403);
        }

        [Fact(DisplayName = "Unauthenticated user adds entry")]
        public async void UnauthenticatedUserAddsEntry()
        {
            var httpClient = this.CreateAnonymousClient();

            var newEntryModel = new CreateEntryModel(new[] { "tag1", "tag2", "tag3" });

            var result = await httpClient.PostAsync("/entry",
                new StringContent(JsonSerializer.Serialize(newEntryModel), Encoding.UTF8,
                    MediaTypeNames.Application.Json));

            result.StatusCode.Should().Be(401);
        }
    }
}
