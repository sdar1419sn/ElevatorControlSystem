using ElevatorControlSystem.Domain.Repositories;
using MediatR;

namespace ElevatorControlSystem.Application.Commands.Handlers;

public class SelectDestinationCommandHandler : IRequestHandler<SelectDestinationCommand>
{
    private readonly IElevatorRepository _elevatorRepository;

    public SelectDestinationCommandHandler(IElevatorRepository elevatorRepository)
    {
        _elevatorRepository = elevatorRepository;
    }

    public async Task Handle(SelectDestinationCommand request, CancellationToken cancellationToken)
    {
        var elevator = await _elevatorRepository.GetByIdAsync(request.ElevatorId);
        if (elevator != null)
        {
            elevator.Destinations.Add(request.DestinationFloor);
            await _elevatorRepository.UpdateAsync(elevator);
        }
    }
}