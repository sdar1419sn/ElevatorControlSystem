using ElevatorControlSystem.Domain.Enums;

namespace ElevatorControlSystem.Application.DTOs;

public class ElevatorStatusDto
{
    public int Id { get; set; }
    public int CurrentFloor { get; set; }
    public Direction Direction { get; set; }
    public List<int> Destinations { get; set; } = new();
    public List<int> Passengers { get; set; } = new();  // Send passenger destinations
}