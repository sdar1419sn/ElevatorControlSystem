using ElevatorControlSystem.Domain.Enums;

namespace ElevatorControlSystem.Domain.Entities;

public class Elevator
{
    public int Id { get; set; }
    public int CurrentFloor { get; set; } = 1;
    public Direction Direction { get; set; } = Direction.Idle;
    public List<int> Destinations { get; set; } = new();
}