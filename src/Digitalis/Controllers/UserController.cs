using System.Threading.Tasks;
using Digitalis.Features;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Digitalis.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("user")]
        public async Task<string> Post([FromBody] CreateUser.Command command) => await _mediator.Send(command);
    }
}
