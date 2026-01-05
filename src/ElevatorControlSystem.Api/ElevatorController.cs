using ElevatorControlSystem.Application.Commands;
using ElevatorControlSystem.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ElevatorControlSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ElevatorController : ControllerBase
{
    private readonly IMediator _mediator;

    public ElevatorController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("request")]
    public async Task<IActionResult> RequestElevator([FromBody] RequestElevatorCommand command)
    {
        await _mediator.Send(command);
        return Ok();
    }

    [HttpPost("destination")]
    public async Task<IActionResult> SelectDestination([FromBody] SelectDestinationCommand command)
    {
        await _mediator.Send(command);
        return Ok();
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var status = await _mediator.Send(new GetElevatorStatusQuery());
        return Ok(status);
    }
}