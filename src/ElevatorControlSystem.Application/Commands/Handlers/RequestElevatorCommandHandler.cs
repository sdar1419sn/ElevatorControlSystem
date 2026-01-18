using ElevatorControlSystem.Domain.Entities;
using ElevatorControlSystem.Domain.Enums;
using ElevatorControlSystem.Domain.Repositories;
using MediatR;

namespace ElevatorControlSystem.Application.Commands.Handlers;

public class RequestElevatorCommandHandler : IRequestHandler<RequestElevatorCommand> 
{
    private readonly IElevatorRepository _elevatorRepository;
    private readonly IFloorRequestRepository _floorRequestRepository;

    public RequestElevatorCommandHandler(IElevatorRepository elevatorRepository, IFloorRequestRepository floorRequestRepository)
    {
        _elevatorRepository = elevatorRepository;
        _floorRequestRepository = floorRequestRepository;
    }

    public async Task Handle(RequestElevatorCommand request, CancellationToken cancellationToken)
    {
        // Simple optimization: Find nearest elevator in suitable direction or idle 
        var elevators = await _elevatorRepository.GetAllAsync();
        var suitableElevator = elevators
            .OrderBy(e => Math.Abs(e.CurrentFloor - request.Floor))
            .FirstOrDefault(e => e.Direction == request.Direction || e.Direction == Direction.Idle) ?? elevators.First();

        var floorRequest = new FloorRequest { Floor = request.Floor, Direction = request.Direction, AssignedElevatorId = suitableElevator.Id };
        await _floorRequestRepository.AddAsync(floorRequest);

        Console.WriteLine($"{request.Direction} request on floor {request.Floor} received");
    }
}