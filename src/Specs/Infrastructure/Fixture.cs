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
        //protected readonly HttpClient HttpClient;
        protected readonly IDocumentStore Store;
        protected readonly WebApplicationFactory<Startup> Factory;

        public Fixture(WebApplicationFactory<Startup> factory)
        {
            Factory = factory;

            Store = this.GetDocumentStore();
            IndexCreation.CreateIndexes(typeof(Startup).Assembly, Store);

            //HttpClient = factory
            //    .CreateClientWithTestAuth()
            //    ;

            //HttpClient = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(
            //    services => services.AddMvc(
            //        options =>
            //        {
            //            options.Filters.Add(new AllowAnonymousFilter());
            //            options.Filters.Add(new FakeUserFilter());
            //        }))).CreateClient();


            //HttpClient = factory.WithWebHostBuilder(builder =>
            //    {
            //        builder.ConfigureTestServices(services =>
            //        {
            //            services.AddSingleton<IDocumentStore>(Store);
            //        });
            //    })
            //    .CreateDefaultClient();


            //HttpClient = factory.WithAuthentication()
            //builder =>
            //    {
            //        builder.ConfigureTestServices(services =>
            //        {
            //            services.AddSingleton<IDocumentStore>(Store);
            //        });
            //    })
            //.CreateDefaultClient();
            //    .CreateClientWithTestAuth();

            //var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            //{
            //    AllowAutoRedirect = false
            //});

            //HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
        }

        public HttpClient GetAuthenticatedClient()
        {
            return this.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(
                services => services.AddMvc(
                    options =>
                    {
                        options.Filters.Add(new AllowAnonymousFilter());
                        options.Filters.Add(new FakeUserFilter());
                    }))).CreateClient();
        }
    }

    public class FakeUserFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            context.HttpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, "12345678-1234-1234-1234-123456789012"),
                    new Claim(ClaimTypes.Name, "TestUser"),
                    new Claim(ClaimTypes.Email, "test.user@example.com"), // add as many claims as you need
                }
                , "TestAuth"
                )
            );

            await next();
        }
    }
}
