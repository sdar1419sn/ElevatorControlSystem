using MediatR;

namespace ElevatorControlSystem.Application.Commands;

public record SelectDestinationCommand(int ElevatorId, int DestinationFloor) : IRequest;