using ElevatorControlSystem.Domain.Enums;

namespace ElevatorControlSystem.Domain.Entities;

public class FloorRequest
{
    public int Id { get; set; }
    public int Floor { get; set; }
    public Direction Direction { get; set; }
    public int? AssignedElevatorId { get; set; }
}