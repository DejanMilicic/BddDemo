﻿using System.Collections.Generic;
using System.Net;
using System.Net.Http;
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
    [Trait("Fetch entry", "Anon User")]
    public class AnonUser : Fixture
    {
        private readonly HttpResponseMessage _response;
        private readonly CreateEntry.Command _newEntry;
        private readonly string _newEntryId;
        private readonly Entry _fetchedEntry;

        public AnonUser()
        {
            User user = new User
            {
                Email = "admin@app.com",
                Claims = new List<(string, string)> { (AppClaims.CreateNewEntry, "") }
            };

            using var session = Store.OpenSession();
            session.Store(user);
            session.SaveChanges();

            var creatorClient = AuthClient(new Dictionary<string, string>
            {
                { "email", user.Email }
            });

            var readerClient = Client();

            _newEntry = new CreateEntry.Command{ Tags = new[] { "tag1", "tag2", "tag3" }};

            _response = creatorClient.PostAsync("/entry",
                Serialize(_newEntry)).Result;

            _newEntryId = _response.Content.ReadAsStringAsync().Result;

            _response = readerClient.GetAsync($"/entry?id={_newEntryId}").Result;

            _fetchedEntry = Deserialize<Entry>(_response);

            WaitForIndexing(Store);
            WaitForUserToContinueTheTest(Store);
        }

        [Fact(DisplayName = "1. Status 401 is returned")]
        public void ForbiddenReturned()
        {
            _response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact(DisplayName = "2. Entry is not returned")]
        public void EntryNotReturned()
        {
            _fetchedEntry.Should().BeNull();
        }
    }
}
