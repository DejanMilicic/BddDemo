using Digitalis.Infrastructure;
using FluentValidation.AspNetCore;
using Hellang.Middleware.ProblemDetails;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NJsonSchema.Generation;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Session;
using System;
using System.Net.Http;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Digitalis.Services;

namespace Digitalis
{
    internal class CustomSchemaNameGenerator : ISchemaNameGenerator
    {
        public string Generate(Type type)
        {
            return type.FullName.Replace(".", "_");
        }
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();
            services.AddControllers()
                .AddFluentValidation(s => s.RegisterValidatorsFromAssemblyContaining<Startup>());

            services.AddSingleton<IDocumentStore>(_ =>
            {
                var dbConfig = Configuration.GetSection("Database").Get<Settings.DatabaseSettings>();

                var store = new DocumentStore
                {
                    Urls = dbConfig.Urls,
                    Database = dbConfig.DatabaseName
                };

                if (!string.IsNullOrWhiteSpace(dbConfig.CertPath))
                    store.Certificate = new X509Certificate2(dbConfig.CertPath, dbConfig.CertPass);

                store.Initialize();

                IndexCreation.CreateIndexes(typeof(Startup).Assembly, store);

                return store;
            });

            services.AddScoped<IAsyncDocumentSession>(sp => sp.GetService<IDocumentStore>()?.OpenAsyncSession());
            services.AddScoped<IDocumentSession>(sp => sp.GetService<IDocumentStore>()?.OpenSession());

            services.AddMediatR(typeof(Startup));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizerPipelineBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidatorPipelineBehavior<,>));

            services.AddAuthorization();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.LoginPath = new PathString("/auth/login");
                        options.AccessDeniedPath = new PathString("/auth/denied");
                    });

            services.Scan(
                x =>
                {
                    var entryAssembly = Assembly.GetAssembly(typeof(Startup));

                    x.FromAssemblies(entryAssembly)
                        .AddClasses(classes => classes.AssignableTo(typeof(IAuthorizer<>)))
                        .AsImplementedInterfaces()
                        .WithScopedLifetime();
                });

            services.AddTransient<IMailer, Mailer>();

            services.AddProblemDetails(ConfigureProblemDetails);

            //services.AddOpenApiDocument(cfg => { cfg.SchemaNameGenerator = new CustomSchemaNameGenerator(); });
            services.AddSwaggerDocument(cfg => { cfg.SchemaNameGenerator = new CustomSchemaNameGenerator(); });
        }

        private void ConfigureProblemDetails(ProblemDetailsOptions options)
        {
            options.Rethrow<NotSupportedException>();

            options.MapToStatusCode<NotImplementedException>(StatusCodes.Status501NotImplemented);
            options.MapToStatusCode<AuthenticationException>(StatusCodes.Status401Unauthorized);
            options.MapToStatusCode<UnauthorizedAccessException>(StatusCodes.Status403Forbidden);
            options.MapToStatusCode<InputValidationException>(StatusCodes.Status400BadRequest);

            options.MapToStatusCode<HttpRequestException>(StatusCodes.Status503ServiceUnavailable);

            // Because exceptions are handled polymorphically, this will act as a "catch all" mapping, which is why it's added last.
            // If an exception other than NotImplementedException and HttpRequestException is thrown, this will handle it.
            options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseProblemDetails();

            if (env.IsDevelopment())
            {
                //app.UseDeveloperExceptionPage();
                //app.UseSwagger();
                //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Digitalis v1"));
            }

            // Add OpenAPI/Swagger middlewares
            app.UseOpenApi(); // Serves the registered OpenAPI/Swagger documents by default on `/swagger/{documentName}/swagger.json`
                              //app.UseSwaggerUi3(); // Serves the Swagger UI 3 web ui to view the OpenAPI/Swagger documents by default on `/swagger`
                              //app.UseSwaggerUi3(settings =>
                              //{
                              //    settings.GeneratorSettings.DefaultPropertyNameHandling = PropertyNameHandling.CamelCase;
                              //    settings.GeneratorSettings.DefaultUrlTemplate = "{controller=Home}/{action=Index}/{locale?}";
                              //    settings.GeneratorSettings.IsAspNetCore = true;
                              //});
            //app.UseSwaggerUi3();
            app.UseSwaggerUi3(options =>
            {
                options.Path = "/openapi";
                options.DocumentPath = "/openapi/v1/openapi.json";
            });
            app.UseSwaggerUi3(settings =>
            {
                settings.DocExpansion = "list";
                //settings.GeneratorSettings.SchemaType = SchemaType.OpenApi3;
                //settings.GeneratorSettings.DefaultUrlTemplate = "api/{controller}/{id?}";
                //settings.GeneratorSettings.GenerateExamples = true;
                //settings.PostProcess = document =>
                //{
                //    document.Info.Title = "Internal API";
                //    document.Info.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                //};
            });

            app.UseReDoc(config =>
            {
                config.Path = "/redoc";
                config.DocumentPath = "/swagger/v1/swagger.json";
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/healthcheck");
                endpoints.MapControllers();
            });
        }
    }
}
