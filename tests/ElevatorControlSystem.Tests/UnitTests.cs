using ElevatorControlSystem.Application.Commands;
using ElevatorControlSystem.Application.Commands.Handlers;
using ElevatorControlSystem.Domain.Entities;
using ElevatorControlSystem.Domain.Enums;
using ElevatorControlSystem.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ElevatorControlSystem.Tests;

public class UnitTests
{
    [Fact]
    public async Task RequestElevatorCommandHandler_AssignsNearestElevator()
    {
        // Arrange
        var elevatorRepoMock = new Mock<IElevatorRepository>();
        var requestRepoMock = new Mock<IFloorRequestRepository>();
        var elevators = new List<Elevator>
        {
            new() { Id = 1, CurrentFloor = 2, Direction = Direction.Idle },
            new() { Id = 2, CurrentFloor = 5, Direction = Direction.Up }
        };
        elevatorRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(elevators);
        var handler = new RequestElevatorCommandHandler(elevatorRepoMock.Object, requestRepoMock.Object);

        // Act
        await handler.Handle(new RequestElevatorCommand(3, Direction.Up), default);

        // Assert
        requestRepoMock.Verify(r => r.AddAsync(It.Is<FloorRequest>(fr => fr.AssignedElevatorId == 1)), Times.Once);
    }

    [Fact]
    public async Task SelectDestinationCommandHandler_AddsDestination()
    {
        // Arrange
        var elevatorRepoMock = new Mock<IElevatorRepository>();
        var elevator = new Elevator { Id = 1, Destinations = new List<int>() };
        elevatorRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(elevator);
        var handler = new SelectDestinationCommandHandler(elevatorRepoMock.Object);

        // Act
        await handler.Handle(new SelectDestinationCommand(1, 5), default);

        // Assert
        Assert.Contains(5, elevator.Destinations);
        elevatorRepoMock.Verify(r => r.UpdateAsync(elevator), Times.Once);
    }

    // Add more tests for queries, simulation logic, etc.
    [Fact]
    public async Task GetElevatorStatusQueryHandler_ReturnsStatuses()
    {
        // Arrange
        var elevatorRepoMock = new Mock<IElevatorRepository>();
        var elevators = new List<Elevator> { new() { Id = 1, CurrentFloor = 3 } };
        elevatorRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(elevators);
        var handler = new Application.Queries.Handlers.GetElevatorStatusQueryHandler(elevatorRepoMock.Object);

        // Act
        var result = await handler.Handle(new Application.Queries.GetElevatorStatusQuery(), default);

        // Assert
        Assert.Single(result);
        Assert.Equal(3, result[0].CurrentFloor);
    }

    [Fact]
    public async Task Elevator_DropsOffPassenger_WhenArrivesAtDestination()
    {
        // Arrange - setup mocks
        var elevatorRepoMock = new Mock<IElevatorRepository>();
        var requestRepoMock = new Mock<IFloorRequestRepository>();

        var elevator = new Elevator
        {
            Id = 1,
            CurrentFloor = 4,
            Direction = Direction.Up,
            Destinations = new List<int> { 5 },
            Passengers = new List<int> { 5 }
        };

        elevatorRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Elevator> { elevator });
        elevatorRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Elevator>())).Returns(Task.CompletedTask); 

        requestRepoMock.Setup(r => r.GetPendingAsync()).ReturnsAsync(new List<FloorRequest>());

        var service = new ElevatorSimulationService(null!); // Mock DI not needed for this

        // Force currentFloor to 5 for test
        elevator.CurrentFloor = 5;

        // Call the method that handles arrival
        await service.ProcessElevator(elevator, new List<FloorRequest>(), elevatorRepoMock.Object, requestRepoMock.Object);

        // Assert
        Assert.Empty(elevator.Passengers);
        Assert.Equal(0, elevator.Passengers.Count);
        Assert.DoesNotContain(5, elevator.Destinations);
    }
}