using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Digitalis;
using Digitalis.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.TestDriver;
using Xunit;

namespace Specs.Infrastructure
{
    public class Fixture : RavenTestDriver, IClassFixture<WebApplicationFactory<Startup>>
    {
        static Fixture()
        {
            ConfigureServer(new TestServerOptions
            {
                FrameworkVersion = null
            });
        }

        protected readonly IDocumentStore Store;
        protected readonly WebApplicationFactory<Startup> Factory;
        protected readonly IMailer Mailer;

        public Fixture(WebApplicationFactory<Startup> factory)
        {
            Factory = factory;

            Mailer = A.Fake<IMailer>();

            Console.WriteLine("Creating... " + GetType().Name);

            Store = GetDocumentStore();

            Console.WriteLine("Created... " + GetType().Name);

            //IndexCreation.CreateIndexes(typeof(Startup).Assembly, Store);
        }

        public override void Dispose()
        {
            Console.WriteLine("Disposing... " + GetType().Name);
            base.Dispose();
            Console.WriteLine("Disposed... " + GetType().Name);
        }

        public HttpClient CreateAuthenticatedClient(IEnumerable<Claim> claims)
        {
            return this.Factory.WithWebHostBuilder(builder =>
            {
                 builder.ConfigureTestServices(services =>
                 {
                     services.AddControllers(
                         options =>
                         {
                             options.Filters.Add(new FakeUserFilter(claims));
                         });

                     services.AddSingleton<IDocumentStore>(Store);
                     services.AddTransient<IMailer>(sp => Mailer);
                 });
            }).CreateClient();
        }

        public HttpClient CreateAnonymousClient()
        {
            return this.Factory.WithWebHostBuilder(builder =>
            {
                 builder.ConfigureTestServices(services =>
                 {
                     services.AddSingleton<IDocumentStore>(Store);
                     services.AddTransient<IMailer>(sp => Mailer);
                 });
            }).CreateClient();
        }

        public StringContent Serialize<T>(T obj)
        {
            return new StringContent(
                JsonSerializer.Serialize(obj),
                Encoding.UTF8,
                MediaTypeNames.Application.Json);
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
