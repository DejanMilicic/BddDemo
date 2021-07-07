using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Digitalis.Features;
using Digitalis.Models;
using FakeItEasy;
using FluentAssertions;
using Raven.Client.Documents.Session;
using Specs.Infrastructure;
using Xunit;

namespace Specs.Features.CreateUser
{
    [Trait("Add New User", "Anon User")]
    public class AnonUser : Fixture
    {
        private readonly HttpResponseMessage _response;
        private readonly Digitalis.Features.CreateUser.Command _newUser;

        public AnonUser()
        {
            var client = Client();

            _newUser = new Digitalis.Features.CreateUser.Command{Email = "john@doe.com", Claims = new Dictionary<string, string>()};

            _response = client.PostAsync("/user",
                Serialize(_newUser)).Result;

            WaitForIndexing(Store);
            WaitForUserToContinueTheTest(Store);
        }

        [Fact(DisplayName = "1. Status 401 is returned")]
        public void StatusReturned()
        {
            _response.StatusCode.Should().Be(401);
        }
    }
}
