using ElevatorControlSystem.Domain.Entities;

namespace ElevatorControlSystem.Domain.Repositories;

public interface IFloorRequestRepository
{
    Task AddAsync(FloorRequest request);
    Task<List<FloorRequest>> GetPendingAsync();
    Task RemoveAsync(FloorRequest request);
    
    Task UpdateAsync(FloorRequest request);
}