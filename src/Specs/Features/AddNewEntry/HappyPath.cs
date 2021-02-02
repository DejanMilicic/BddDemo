using Digitalis;
using Digitalis.Features;
using Digitalis.Models;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Raven.Client.Documents.Session;
using Specs.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
            var client = AuthClient(new Dictionary<string, string>
                {
                    { "CreateNewEntry", "" }
                });

            _newEntry = new CreateEntry.Command(new[] { "tag1", "tag2", "tag3" });

            _response = client.PostAsync("/entry",
                Serialize(_newEntry)).Result;

            WaitForIndexing(Store);
            WaitForUserToContinueTheTest(Store);
        }

        [Fact(DisplayName = "1. Status 200 is returned")]
        public void StatusReturned()
        {
            _response.StatusCode.Should().Be(200);
        }

        [Fact(DisplayName = "2. One entry is created in the database")]
        public void OneEntryCreated()
        {
            Store.OpenSession().Query<Entry>().Statistics(out QueryStatistics stats).ToList();

            stats.TotalResults.Should().Be(1);
        }

        [Fact(DisplayName = "3. New entry has expected content")]
        public void ExpectedContent()
        {
            var entry = Store.OpenSession().Query<Entry>().Single();

            entry.Tags.Should().BeEquivalentTo(_newEntry.Tags);
        }

        [Fact(DisplayName = "4. One email is sent")]
        public void OneEmailSent()
        {
            A.CallTo(() => Mailer
                    .SendMail(
                        A<string>.Ignored,
                        A<string>.Ignored,
                        A<string>.Ignored))
                    .MustHaveHappenedOnceExactly();
        }

        [Fact(DisplayName = "5. Email is addressed to admin")]
        public void EmailAddressedToAdmin()
        {
            A.CallTo(() => Mailer
                    .SendMail(
                        A<string>.That.Matches(x => x == "admin@site.com"),
                        A<string>.Ignored,
                        A<string>.Ignored))
                    .MustHaveHappenedOnceExactly();
        }

        [Fact(DisplayName = "6. Email subject is correct")]
        public void EmailSubjectIsCorrect()
        {
            A.CallTo(() => Mailer
                    .SendMail(
                        A<string>.Ignored,
                        A<string>.That.Matches(x => x == "New entry created"),
                        A<string>.Ignored))
                    .MustHaveHappenedOnceExactly();
        }

        [Fact(DisplayName = "7. Email body contains all tags")]
        public void EmailBodyContainsAllTags()
        {
            foreach (string tag in _newEntry.Tags)
            {
                A.CallTo(() => Mailer
                        .SendMail(
                            A<string>.Ignored,
                            A<string>.Ignored,
                            A<string>.That.Matches(x => _newEntry.Tags.All(x.Contains))))
                        .MustHaveHappenedOnceExactly();
            }
        }
    }
}
