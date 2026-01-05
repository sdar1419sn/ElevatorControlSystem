using ElevatorControlSystem.Domain.Enums;
using MediatR;

namespace ElevatorControlSystem.Application.Commands;

public record RequestElevatorCommand(int Floor, Direction Direction) : IRequest;