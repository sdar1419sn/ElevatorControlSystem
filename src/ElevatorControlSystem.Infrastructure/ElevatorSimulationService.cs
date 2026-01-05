using ElevatorControlSystem.Domain.Entities;
using ElevatorControlSystem.Domain.Enums;
using ElevatorControlSystem.Domain.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ElevatorControlSystem.Infrastructure;

public class ElevatorSimulationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Random _random = new();

    public ElevatorSimulationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var elevatorRepo = scope.ServiceProvider.GetRequiredService<IElevatorRepository>();
            var requestRepo = scope.ServiceProvider.GetRequiredService<IFloorRequestRepository>();

            // Generate random request every 5-15 seconds
            if (_random.Next(0, 3) == 0)
            {
                var floor = _random.Next(1, 11);
                var direction = _random.Next(0, 2) == 0 ? Direction.Up : Direction.Down;
                var request = new FloorRequest { Floor = floor, Direction = direction };
                await requestRepo.AddAsync(request);
                Console.WriteLine($"{direction} request on floor {floor} received (simulated)");
            }

            // Process elevators
            var elevators = await elevatorRepo.GetAllAsync();
            var pendingRequests = await requestRepo.GetPendingAsync();

            foreach (var elevator in elevators)
            {
                await ProcessElevator(elevator, pendingRequests, elevatorRepo, requestRepo);
            }

            // Log positions
            LogElevatorPositions(elevators);

            await Task.Delay(5000, stoppingToken); // Simulate tick every 5s
        }
    }

    private async Task ProcessElevator(Elevator elevator, List<FloorRequest> pendingRequests, IElevatorRepository elevatorRepo, IFloorRequestRepository requestRepo)
    {
        if (elevator.Direction == Direction.Idle && pendingRequests.Any())
        {
            // Assign nearest request
            var nearestRequest = pendingRequests.OrderBy(r => Math.Abs(r.Floor - elevator.CurrentFloor)).First();
            elevator.Direction = nearestRequest.Floor > elevator.CurrentFloor ? Direction.Up : Direction.Down;
            elevator.Destinations.Add(nearestRequest.Floor);
            await requestRepo.RemoveAsync(nearestRequest);
            await elevatorRepo.UpdateAsync(elevator);
        }

        if (elevator.Destinations.Any())
        {
            var nextDestination = elevator.Direction == Direction.Up
                ? elevator.Destinations.Where(d => d > elevator.CurrentFloor).MinBy(d => d)
                : elevator.Destinations.Where(d => d < elevator.CurrentFloor).MinBy(d => elevator.CurrentFloor - d);

            if (nextDestination == null)
            {
                // Change direction if no more in current direction
                elevator.Direction = elevator.Direction == Direction.Up ? Direction.Down : Direction.Up;
                nextDestination = elevator.Direction == Direction.Up
                    ? elevator.Destinations.Where(d => d > elevator.CurrentFloor).MinBy(d => d)
                    : elevator.Destinations.Where(d => d < elevator.CurrentFloor).MinBy(d => elevator.CurrentFloor - d);
            }

            if (nextDestination != null)
            {
                // Move towards next (simulate 10s per floor)
                await Task.Delay(10000); // Simulate move time
                elevator.CurrentFloor += elevator.CurrentFloor < nextDestination ? 1 : -1;

                if (elevator.CurrentFloor == nextDestination)
                {
                    // Stop, passengers enter/leave (10s)
                    await Task.Delay(10000);
                    elevator.Destinations.Remove(nextDestination);
                    Console.WriteLine($"Elevator {elevator.Id} stopped on floor {elevator.CurrentFloor} for passengers");
                }

                await elevatorRepo.UpdateAsync(elevator);
            }
            else
            {
                elevator.Direction = Direction.Idle;
                await elevatorRepo.UpdateAsync(elevator);
            }
        }
    }

    private void LogElevatorPositions(List<Elevator> elevators)
    {
        foreach (var e in elevators)
        {
            Console.WriteLine($"Car {e.Id} is on floor {e.CurrentFloor}");
        }
    }
}