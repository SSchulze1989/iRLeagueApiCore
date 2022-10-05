using iRLeagueApiCore.Server.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace iRLeagueApiCore.Server.Controllers
{
    public sealed class UsersController : LeagueApiController<UsersController>
    {
        public UsersController(ILogger<UsersController> logger, IMediator mediator) : base(logger, mediator)
        {
        }
    }
}
