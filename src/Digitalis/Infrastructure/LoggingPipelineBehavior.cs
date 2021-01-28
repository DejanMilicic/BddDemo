using System.Threading;
using System.Threading.Tasks;
using MediatR;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Digitalis.Infrastructure
{
    public class LoggingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            TResponse response = default(TResponse);

            long start = Stopwatch.GetTimestamp();
            long stop = 0;
            try
            {
                response = await next();
                stop = Stopwatch.GetTimestamp();
                return response;
            }
            catch
            {
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
                string actorEmail = actor.Actor != null ? actor.Actor.Email : "unknown actor";

                Log.Information("{Actor} {Input} {Output} {Elapsed}", actorEmail, serializedRequest, serializedResponse,
                    elapsed);
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
