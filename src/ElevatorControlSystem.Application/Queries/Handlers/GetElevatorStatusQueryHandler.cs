using ElevatorControlSystem.Application.DTOs;
using ElevatorControlSystem.Domain.Repositories;
using MediatR;

namespace ElevatorControlSystem.Application.Queries.Handlers;

public class GetElevatorStatusQueryHandler : IRequestHandler<GetElevatorStatusQuery, List<ElevatorStatusDto>>
{
    private readonly IElevatorRepository _elevatorRepository;

    public GetElevatorStatusQueryHandler(IElevatorRepository elevatorRepository)
    {
        _elevatorRepository = elevatorRepository;
    }

    public async Task<List<ElevatorStatusDto>> Handle(GetElevatorStatusQuery request, CancellationToken cancellationToken)
    {
        var elevators = await _elevatorRepository.GetAllAsync();
        return elevators.Select(e => new ElevatorStatusDto
        {
            Id = e.Id,
            CurrentFloor = e.CurrentFloor,
            Direction = e.Direction,
            Destinations = e.Destinations
        }).ToList();
    }
}