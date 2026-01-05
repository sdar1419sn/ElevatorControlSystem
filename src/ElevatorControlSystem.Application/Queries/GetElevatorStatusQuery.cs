using ElevatorControlSystem.Application.DTOs;
using MediatR;

namespace ElevatorControlSystem.Application.Queries;

public record GetElevatorStatusQuery : IRequest<List<ElevatorStatusDto>>;