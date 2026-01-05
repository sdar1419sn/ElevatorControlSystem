using ElevatorControlSystem.Domain.Entities;

namespace ElevatorControlSystem.Domain.Repositories;

public interface IElevatorRepository
{
    Task<List<Elevator>> GetAllAsync();
    Task<Elevator?> GetByIdAsync(int id);
    Task UpdateAsync(Elevator elevator);
}