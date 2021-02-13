using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Digitalis.Infrastructure.Mediatr
{
    internal class LoggingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IHttpContextAccessor _ctx;

        public LoggingPipelineBehavior(IHttpContextAccessor ctx)
        {
            _ctx = ctx;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            TResponse response = default(TResponse);

            long start = Stopwatch.GetTimestamp();
            long stop = 0;
            int category = 2;
            try
            {
                response = await next();
                stop = Stopwatch.GetTimestamp();
                return response;
            }
            catch (Exception ex)
            {
                category = 5;
                // log exception here
                throw;
            }
            finally
            {
                //Log.Information("{MediatR}", Serialize(request, response, GetElapsedMilliseconds(start, stop)));

                var serializedRequest = JsonConvert.SerializeObject(request);
                var serializedResponse = JsonConvert.SerializeObject(response);

                string elapsed = "";
                if (stop != 0)
                    elapsed = $"{GetElapsedMilliseconds(start, stop):0.0000}";

                dynamic actor = JObject.Parse(serializedRequest);
                //string actorEmail = actor.Actor != null ? actor.Actor.Email : _ctx.HttpContext.Connection.RemoteIpAddress.ToString();
                string actorEmail = _ctx.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "";

                switch (category)
                {
                    case 2:
                        Log.Information("{Actor} {Input} {Output} {Elapsed}", 
                            actorEmail, serializedRequest, serializedResponse, elapsed);
                        break;
                    case 5:
                        Log.Error("{Actor} {Input} {Output} {Elapsed}", 
                            actorEmail, serializedRequest, serializedResponse, elapsed);
                        break;
                    default: break;
                }

            }
        }

        private Dictionary<string, string> Serialize(TRequest request, TResponse result, double elapsedMs)
        {
            var serializationSettings =
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

            return new Dictionary<string, string>
            {
                {"Elapsed", $"{elapsedMs:0.0000}"},
                {"Type", request.GetType().FullName},
                //{"Type", request.GetType().GetFields()},
                {"Request", JsonConvert.SerializeObject(request, serializationSettings)},
                {"Response", JsonConvert.SerializeObject(result, serializationSettings)}
            };
        }

        private Dictionary<string, string> Serialize(object obj, string name)
        {
            var serializationSettings =
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

            return new Dictionary<string, string>
            {
                {name, JsonConvert.SerializeObject(obj, serializationSettings)}
            };
        }

        private double GetElapsedMilliseconds(long start, long stop)
        {
            return (stop - start) * 1000 / (double)Stopwatch.Frequency;
        }
    }
}
