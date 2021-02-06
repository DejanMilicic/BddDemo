using Digitalis.Models;
using MediatR;
using Newtonsoft.Json;

namespace Digitalis.Features
{
    public class Request<TResponse> : IRequest<TResponse>
    {
        public virtual void Authorize()
        {

        }
    }
}
