using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Digitalis;
using Digitalis.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.TestDriver;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Specs.Infrastructure
{
    public class Fixture : RavenTestDriver, IClassFixture<WebApplicationFactory<Startup>>
    {
        protected readonly IDocumentStore Store;
        protected readonly WebApplicationFactory<Startup> Factory;
        protected readonly IMailer Mailer;

        public Fixture(WebApplicationFactory<Startup> factory)
        {
            Factory = factory;

            Mailer = A.Fake<IMailer>();

            Store = this.GetDocumentStore();
            IndexCreation.CreateIndexes(typeof(Startup).Assembly, Store);
        }

        public HttpClient CreateAuthenticatedClient(IEnumerable<Claim> claims = null)
        {
            claims ??= Enumerable.Empty<Claim>();

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

        public T Deserialize<T>(HttpResponseMessage response)
        {
            if (response.StatusCode != HttpStatusCode.OK)
                return default(T);

            string content = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<T>(content);
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
