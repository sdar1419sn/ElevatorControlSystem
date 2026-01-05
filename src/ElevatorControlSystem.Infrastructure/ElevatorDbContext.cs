using ElevatorControlSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ElevatorControlSystem.Infrastructure;

public class ElevatorDbContext : DbContext
{
    public DbSet<Elevator> Elevators { get; set; }
    public DbSet<FloorRequest> FloorRequests { get; set; }

    public ElevatorDbContext(DbContextOptions<ElevatorDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Seed elevators
        for (int i = 1; i <= 4; i++)
        {
            modelBuilder.Entity<Elevator>().HasData(new Elevator { Id = i, CurrentFloor = 1 });
        }
    }
}