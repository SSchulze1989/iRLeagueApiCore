using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.AdminPanel;
using iRLeagueApiCore.Server.Models.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iRLeagueApiCore.Server.Controllers.AdminPanel;

[ApiController]
[TypeFilter(typeof(DefaultExceptionFilterAttribute))]
[Authorize]
[Route("AdminPanel/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator mediator;

    public PaymentsController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    /// <summary>
    /// Get payments from customers
    /// </summary>
    /// <param name="leagueId">[Optional] If set: only show payments done for this league</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentModel>>> Get([FromQuery] long? leagueId, CancellationToken cancellationToken)
    {
        var request = new GetAllPaymentsRequest(leagueId);
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("id:Guid")]
    public async Task<ActionResult<PaymentModel>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var request = new GetPaymentRequest(id);
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<PaymentModel>> Post([FromQuery] long leagueId, [FromBody] PostPaymentModel model, CancellationToken cancellationToken)
    {
        var request = new PostPaymentRequest(leagueId, model);
        var result = await mediator.Send(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
