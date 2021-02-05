using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using Digitalis;
using Digitalis.Infrastructure;
using Digitalis.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.TestDriver;
using Serilog;
using WebMotions.Fake.Authentication.JwtBearer;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Specs.Infrastructure
{
    public class Fixture : RavenTestDriver, IClassFixture<WebApplicationFactory<Startup>>
    {
        protected readonly IDocumentStore Store;
        protected readonly IMailer Mailer;
        protected readonly TestServer TestServer;

        public Fixture()
        {
            Mailer = A.Fake<IMailer>();

            Store = this.GetDocumentStore();
            IndexCreation.CreateIndexes(typeof(Startup).Assembly, Store);

            TestServer = SetupHost().GetTestServer();
        }

        public HttpClient Client()
        {
            return TestServer.CreateClient();
        }

        public HttpClient AuthClient(Dictionary<string, string> claims)
        {
            var client = TestServer.CreateClient();

            client.SetFakeBearerToken(GetClaims(claims));

            return client;


            object GetClaims(Dictionary<string, string> dict)
            {
                dynamic eo = dict.Aggregate(new ExpandoObject() as IDictionary<string, Object>,
                    (a, p) => { a.Add(p.Key, p.Value); return a; });

                return (object)eo;
            }
        }

        public IHost SetupHost()
        {
            return new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.UseStartup<Digitalis.Startup>().UseSerilog();
                    webBuilder
                        .UseTestServer()
                        .UseSetting("Google:ClientId", "client_id")
                        .ConfigureTestServices(collection =>
                        {
                            collection.AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme).AddFakeJwtBearer();
                            collection.AddSingleton<IDocumentStore>(Store);
                            collection.AddTransient<IMailer>(sp => Mailer);
                        });
                })
                .StartAsync().Result;
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
}
