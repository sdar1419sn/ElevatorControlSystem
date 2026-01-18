using ElevatorControlSystem.Application;
using ElevatorControlSystem.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddHostedService<ElevatorSimulationService>();
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
// or even stricter:
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Update", LogLevel.None);

var app = builder.Build();
app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ElevatorDbContext>();
    dbContext.Database.EnsureCreated(); // In-memory DB setup
}
app.Lifetime.ApplicationStarted.Register(() =>
{
    var url = "http://localhost:5000/swagger";
    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
    {
        FileName = url,
        UseShellExecute = true
    });
});

app.Run();