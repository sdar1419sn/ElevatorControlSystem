using ElevatorControlSystem.Domain.Entities;
using ElevatorControlSystem.Domain.Enums;
using ElevatorControlSystem.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        Log("Elevator simulation started → 10 floors, 4 elevators. Generating random hall calls...");

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var elevatorRepo = scope.ServiceProvider.GetRequiredService<IElevatorRepository>();
            var requestRepo = scope.ServiceProvider.GetRequiredService<IFloorRequestRepository>();

            var elevators = await elevatorRepo.GetAllAsync();
            var pendingRequests = await requestRepo.GetPendingAsync();

            // Generate new hall call
            if (_random.NextDouble() < 0.4)
            {
                int floor = _random.Next(1, 11);
                Direction dir = _random.Next(0, 2) == 0 ? Direction.Up : Direction.Down;
                var request = new FloorRequest { Floor = floor, Direction = dir };
                await requestRepo.AddAsync(request);

                Log($"New hall call: {dir} at floor {floor}");
            }

            foreach (var elevator in elevators.OrderBy(e => e.Id))
            {
                await ProcessElevator(elevator, await requestRepo.GetPendingAsync(), elevatorRepo, requestRepo);
            }

            LogElevatorStatus(elevators);

            await Task.Delay(3000, stoppingToken);
        }
    }

    private async Task ProcessElevator(Elevator elevator, List<FloorRequest> pendingRequests,
        IElevatorRepository elevatorRepo, IFloorRequestRepository requestRepo)
    {
        // Assign idle elevator to nearest pending request
        if (elevator.Direction == Direction.Idle && pendingRequests.Any())
        {
            var nearest = pendingRequests
                .OrderBy(r => Math.Abs(r.Floor - elevator.CurrentFloor))
                .ThenBy(r => r.Direction == elevator.Direction ? 0 : 1)
                .First();

            elevator.Direction = nearest.Floor > elevator.CurrentFloor ? Direction.Up : Direction.Down;
            elevator.Destinations.Add(nearest.Floor);

            await requestRepo.RemoveAsync(nearest);
            await elevatorRepo.UpdateAsync(elevator);

            Log($"Elevator {elevator.Id} accepted {nearest.Direction} call at floor {nearest.Floor} → heading {elevator.Direction}");
            return;
        }

        // Process movement or arrival
        if (elevator.Destinations.Any())
        {
            int? nextFloor = elevator.Direction == Direction.Up
                ? elevator.Destinations.Where(d => d >= elevator.CurrentFloor).DefaultIfEmpty().Min()
                : elevator.Destinations.Where(d => d <= elevator.CurrentFloor).DefaultIfEmpty().Max();

            if (!nextFloor.HasValue || nextFloor == elevator.CurrentFloor)
            {
                Log($"Elevator {elevator.Id} arrived at floor {elevator.CurrentFloor} → doors open (2s)");

                // Drop off
                int dropped = elevator.Passengers.RemoveAll(p => p == elevator.CurrentFloor);
                if (dropped > 0)
                {
                    Log($"   → {dropped} passenger(s) disembarked");
                }

                await Task.Delay(2000);

                // Pickup ONLY if waiting request exists here
                var waitingHere = pendingRequests.FirstOrDefault(r => r.Floor == elevator.CurrentFloor);
                if (waitingHere != null)
                {
                    int entering = _random.Next(1, 4);
                    var newDestinations = new List<int>();

                    int minDest = elevator.Direction == Direction.Up ? elevator.CurrentFloor + 1 : 1;
                    int maxDest = elevator.Direction == Direction.Up ? 11 : elevator.CurrentFloor;

                    if (minDest < maxDest)
                    {
                        for (int i = 0; i < entering; i++)
                        {
                            int dest = _random.Next(minDest, maxDest);
                            elevator.Passengers.Add(dest);
                            newDestinations.Add(dest);
                        }

                        Log($"   → {entering} passenger(s) entered → going to {string.Join(", ", newDestinations)}");
                        elevator.Destinations.AddRange(newDestinations);
                    }

                    await requestRepo.RemoveAsync(waitingHere);
                }
                else
                {
                    Log("   → No waiting passengers at this floor");
                }

                elevator.Destinations.Remove(elevator.CurrentFloor);

                if (elevator.Destinations.Any())
                {
                    bool hasMoreInCurrentDir = elevator.Direction == Direction.Up
                        ? elevator.Destinations.Any(d => d > elevator.CurrentFloor)
                        : elevator.Destinations.Any(d => d < elevator.CurrentFloor);

                    if (!hasMoreInCurrentDir)
                    {
                        elevator.Direction = elevator.Direction == Direction.Up ? Direction.Down : Direction.Up;
                        Log($"Elevator {elevator.Id} changed direction to {elevator.Direction} (no more stops in previous direction)");
                    }

                    Log($"Elevator {elevator.Id} doors closed → continuing {elevator.Direction}");
                }
                else
                {
                    elevator.Direction = Direction.Idle;
                    Log($"Elevator {elevator.Id} now idle at floor {elevator.CurrentFloor}");
                }

                await elevatorRepo.UpdateAsync(elevator);
            }
            else
            {
                int step = elevator.Direction == Direction.Up ? 1 : -1;
                elevator.CurrentFloor += step;

                string note = elevator.Passengers.Count == 0 ? " (empty, heading to pick up waiting passengers)" : "";
                Log($"Elevator {elevator.Id} moving {elevator.Direction} → now at floor {elevator.CurrentFloor}{note}");

                await Task.Delay(2000);
                await elevatorRepo.UpdateAsync(elevator);
            }
        }
    }

    private void Log(string message)
    {
        var time = DateTime.Now.ToString("HH:mm:ss");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"[{time}] ");
        Console.ResetColor();
        Console.WriteLine(message);
    }

    private void LogElevatorStatus(IEnumerable<Elevator> elevators)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"\n[{DateTime.Now:HH:mm:ss}] Elevator Status ───────────────────────────────────────");
        Console.ResetColor();

        foreach (var e in elevators)
        {
            string dir = e.Direction switch
            {
                Direction.Up => "↑"+
                Direction.Down => "↓",
                _ => "–"
            };

            string status = e.Destinations.Any() ? "Active" : "Idle";

            Console.WriteLine(
                $"  Car {e.Id,-2}   Floor {e.CurrentFloor,-2}   Dir {dir}   " +
                $"Passengers {e.Passengers.Count,-2}   Destinations: {string.Join(",", e.Destinations),-15}   Status: {status}"
            );
        }

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("───────────────────────────────────────────────────────────────\n");
        Console.ResetColor();
    }
}