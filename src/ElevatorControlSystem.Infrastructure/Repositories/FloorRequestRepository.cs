using ElevatorControlSystem.Domain.Entities;
using ElevatorControlSystem.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ElevatorControlSystem.Infrastructure.Repositories;

public class FloorRequestRepository : IFloorRequestRepository
{
    private readonly ElevatorDbContext _context;

    public FloorRequestRepository(ElevatorDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(FloorRequest request)
    {
        _context.FloorRequests.Add(request);
        await _context.SaveChangesAsync();
    }

    public async Task<List<FloorRequest>> GetPendingAsync()
    {
        return await _context.FloorRequests.ToListAsync();
    }

    public async Task RemoveAsync(FloorRequest request)
    {
        // FIX: Instead of removing detached entity, remove by Id
        // This avoids concurrency issues with InMemory provider
        var entry = await _context.FloorRequests.FindAsync(request.Id);
        if (entry != null)
        {
            _context.FloorRequests.Remove(entry);
            await _context.SaveChangesAsync();
        }
        // If not found, it's already gone — no exception needed
    }

    public async Task UpdateAsync(FloorRequest request)
    {
        _context.FloorRequests.Update(request);
        await _context.SaveChangesAsync();
    }
}