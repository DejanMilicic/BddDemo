using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Digitalis;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.TestDriver;
using Xunit;

namespace Specs.Infrastructure
{
    public class Fixture : RavenTestDriver, IClassFixture<WebApplicationFactory<Startup>>
    {
        protected readonly IDocumentStore Store;
        protected readonly WebApplicationFactory<Startup> Factory;

        public Fixture(WebApplicationFactory<Startup> factory)
        {
            Factory = factory;

            Store = this.GetDocumentStore();
            IndexCreation.CreateIndexes(typeof(Startup).Assembly, Store);
        }

        public HttpClient CreateAuthenticatedClient(IEnumerable<Claim> claims)
        {
            return this.Factory.WithWebHostBuilder(builder =>
            {
                 builder.ConfigureTestServices(services =>
                 {
                     services.AddMvc(
                         options =>
                         {
                             options.Filters.Add(new AllowAnonymousFilter());
                             options.Filters.Add(new FakeUserFilter(claims));
                         });

                     services.AddSingleton<IDocumentStore>(Store);
                 });
            }).CreateClient();
        }
    }

    public class FakeUserFilter : IAsyncActionFilter
    {
        private readonly IEnumerable<Claim> _claims;

        public FakeUserFilter(IEnumerable<Claim> claims)
        {
            _claims = claims;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(_claims, "TestAuthType"));
            await next();
        }
    }
}
