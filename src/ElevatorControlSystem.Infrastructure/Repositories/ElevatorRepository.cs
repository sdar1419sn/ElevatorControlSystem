using ElevatorControlSystem.Domain.Entities;
using ElevatorControlSystem.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ElevatorControlSystem.Infrastructure.Repositories;

public class ElevatorRepository : IElevatorRepository
{
    private readonly ElevatorDbContext _context;

    public ElevatorRepository(ElevatorDbContext context)
    {
        _context = context;
    }

    public async Task<List<Elevator>> GetAllAsync()
    {
        return await _context.Elevators.ToListAsync();
    }

    public async Task<Elevator?> GetByIdAsync(int id)
    {
        return await _context.Elevators.FindAsync(id);
    }

    public async Task UpdateAsync(Elevator elevator)
    {
        _context.Elevators.Update(elevator);
        await _context.SaveChangesAsync();
    }
}