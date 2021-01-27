using System.Net.Http;
using Digitalis;
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
        protected readonly HttpClient HttpClient;
        protected readonly IDocumentStore Store;

        public Fixture(WebApplicationFactory<Startup> factory)
        {
            Store = this.GetDocumentStore();
            IndexCreation.CreateIndexes(typeof(Startup).Assembly, Store);

            HttpClient = factory.WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddSingleton<IDocumentStore>(Store);
                    });
                })
                .CreateDefaultClient();
        }
    }
}
