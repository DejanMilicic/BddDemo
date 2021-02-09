using Digitalis.Infrastructure;
using Hellang.Middleware.ProblemDetails;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Digitalis.Infrastructure.Mediatr;
using Digitalis.Infrastructure.Services;
using Digitalis.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;
using Serilog.Context;
using Serilog.Core.Enrichers;

namespace Digitalis
{
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
            string clientId = Configuration.GetSection("Google").Get<Settings.GoogleSettings>().ClientId;
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(jwt => jwt.UseGoogle(clientId: clientId));

            services.AddHealthChecks();
            services.AddControllers();

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

                DbSeeding dbs = new DbSeeding();
                dbs.Setup(store, Configuration.GetSection("SuperAdmin").Get<string>());

                return store;
            });

            services.AddMediatR(typeof(Startup));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthPipelineBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidatorPipelineBehavior<,>));

            services.AddScoped<Authenticator>();

            var entryAssembly = Assembly.GetAssembly(typeof(Startup));

            services.Scan(
                x =>
                {
                    x.FromAssemblies(entryAssembly)
                        .AddClasses(classes => classes.AssignableTo(typeof(IAuth<>)))
                        .AddClasses(classes => classes.AssignableTo(typeof(IAuth<,>)))
                        .AsImplementedInterfaces()
                        .WithScopedLifetime();
                });

            services.Scan(
                x =>
                {
                    x.FromAssemblies(entryAssembly)
                        .AddClasses(classes => classes.AssignableTo(typeof(AbstractValidator<>)))
                        .AsImplementedInterfaces()
                        .WithScopedLifetime();
                });

            services.AddTransient<IMailer, Mailer>();

            services.AddProblemDetails(ConfigureProblemDetails);

            services.AddSwaggerDocument(settings =>
            {
                settings.Title = App.Title;
            });

            services.AddHttpContextAccessor();
        }

        private void ConfigureProblemDetails(ProblemDetailsOptions options)
        {
            options.Rethrow<NotSupportedException>();

            options.MapToStatusCode<NotImplementedException>(StatusCodes.Status501NotImplemented);
            options.MapToStatusCode<AuthenticationException>(StatusCodes.Status401Unauthorized);
            options.MapToStatusCode<UnauthorizedAccessException>(StatusCodes.Status403Forbidden);
            options.MapToStatusCode<ValidationException>(StatusCodes.Status400BadRequest);
            options.MapToStatusCode<KeyNotFoundException>(StatusCodes.Status404NotFound);

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
            }

            app.Use(async (ctx, next) =>
            {
                using (LogContext.Push(
                    new PropertyEnricher("IPAddress", ctx.Connection.RemoteIpAddress),
                    new PropertyEnricher("RequestHost", ctx.Request.Host),
                    new PropertyEnricher("RequestBasePath", ctx.Request.Path),
                    new PropertyEnricher("RequestQueryParams", ctx.Request.QueryString)))
                {
                    await next();
                }
            });

            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/healthcheck");
                endpoints.MapControllers();
            });

            app.UseOpenApi();
            app.UseSwaggerUi3(settings =>
            {
                settings.DocExpansion = "list";
            });
        }
    }
}
