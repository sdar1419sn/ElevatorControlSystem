Note: For Running project please follow below given steps
after clone repo 
please run dotnet restore
dotnet build
dotnet run --project src\ElevatorControlSystem.Api


ElevatorControlSystem
Complete Design Document

Author: Suhail Ahmad Dar
Date: January 05, 2026
Version: 1.0

Table of Contents
1. Introduction
2. System Overview and Requirements
3. Why We Chose CQRS Pattern
4. High-Level Design (HLD)
5. Architecture and Layering
6. Key Components and Implementation Details
7. Elevator Simulation Logic
8. API Endpoints (Swagger)
9. Testing Strategy
10. Deployment and Running Instructions
11. Future Improvements

1. Introduction
This document describes the design and implementation of ElevatorControlSystem — a .NET 8 Web API that simulates an elementary elevator control system for a 10-floor building with 4 elevators.

The primary focus is on tracking elevator positions, handling floor requests (up/down calls), moving elevators realistically, and providing status via API. Real-world complexities like weight limits, fire modes, door sensors, or emergency overrides are intentionally excluded.

2. System Overview and Requirements
Building Parameters:
- 10 floors (numbered 1 to 10)
- 4 elevator cars
- Movement time: 10 seconds per floor
- Stop time (doors open for passengers): 10 seconds
- Random hall calls generated automatically
- Manual calls possible via API
- Elevators should prefer continuing in one direction (no unnecessary yo-yo)
- Console logging of positions and events

Functional Requirements:
- Simulate elevator movement with realistic timing
- Assign incoming requests to the most suitable elevator (basic optimization)
- Allow querying current status of all elevators
- Provide REST API with Swagger documentation
- No UI required

3. Why We Chose CQRS Pattern
CQRS (Command Query Responsibility Segregation) separates write operations (Commands) from read operations (Queries).

Reasons for choosing CQRS in this project:
- Clear separation of concerns: Commands mutate state (e.g., request elevator, select destination), Queries only read state.
- Scalability potential: In a real system, status queries would be far more frequent than state changes. CQRS allows optimizing reads independently (caching, read models).
- Maintainability: Each operation has a single, focused handler. Easier to test and reason about.
- Future-proofing: Naturally extends to event sourcing, auditing, or separate read/write databases.
- Alignment with domain complexity: Elevator systems involve coordinated state changes and frequent status polling — perfect fit for CQRS.
- MediatR integration: Provides clean pipeline for commands/queries with minimal boilerplate.

Trade-offs acknowledged:
- More files/classes than simple CRUD
- Slight learning curve
Benefits significantly outweigh costs for this domain.

4. High-Level Design (HLD)

<img width="1258" height="672" alt="image" src="https://github.com/user-attachments/assets/d04e0536-e16c-453c-aa33-b3d800b7a605" />
<img width="906" height="498" alt="image" src="https://github.com/user-attachments/assets/4dfaf8f5-a813-417a-9302-c60e92a41822" />

5. Swagger
   <img width="1912" height="793" alt="image" src="https://github.com/user-attachments/assets/d14501a4-08aa-491e-8eb0-d018486ee810" />



Text representation:

Client (Swagger/Postman)
        ↓ (HTTP)
ASP.NET Core Web API Controllers
        ↓
MediatR Dispatcher
        ├─→ Commands → Command Handlers → Repositories → DB
        └─→ Queries  → Query Handlers   → Repositories → DB

Background Service (ElevatorSimulationService)
        ↓ (every 5 seconds)
Repositories ↔ In-Memory Database ↔ Console Logging

5. Architecture and Layering
Clean Architecture (Onion Architecture) with 4 projects:

- ElevatorControlSystem.Domain
  Core entities, enums, repository interfaces (pure business logic, no dependencies)

- ElevatorControlSystem.Application
  CQRS commands, queries, handlers, DTOs
  Uses MediatR for dispatching

- ElevatorControlSystem.Infrastructure
  EF Core DbContext, repository implementations
  Background simulation service (IHostedService)

- ElevatorControlSystem.Api
  Controllers, Program.cs, Swagger configuration
  Entry point

6. Key Components and Implementation Details
Domain Entities:
- Elevator: Id, CurrentFloor, Direction (Up/Down/Idle), Destinations (List<int>)
- FloorRequest: Id, Floor, Direction, AssignedElevatorId

Commands:
- RequestElevatorCommand (Floor, Direction) → assigns nearest suitable elevator
- SelectDestinationCommand (ElevatorId, DestinationFloor) → adds to elevator's destinations

Query:
- GetElevatorStatusQuery → returns list of ElevatorStatusDto

Repositories:
- IElevatorRepository, IFloorRequestRepository (abstracted data access)
- Implemented with EF Core In-Memory provider

Database:
- ElevatorDbContext with DbSets for Elevators and FloorRequests
- Seeded with 4 elevators on floor 1

7. Elevator Simulation Logic
Runs as BackgroundService (ElevatorSimulationService):
- Executes loop every ~5 seconds
- Randomly generates hall calls (floor 1-10, Up/Down)
- For each elevator:
   - If idle and pending requests → assign nearest, set direction, add destination
   - If has destinations → determine next floor in current direction
   - Simulate movement: Task.Delay(10000 × floors)
   - Update current floor
   - Simulate stop: Task.Delay(10000), remove destination, log arrival
   - If no more stops in direction → switch direction or go idle
- Logs current positions every tick

Algorithm highlights:
- Directional persistence (continues Up until no higher stops)
- Basic optimization: nearest elevator with direction preference

8. API Endpoints (Swagger)
Base URL: /api/Elevator

- GET    /status              → Returns current status of all elevators
- POST   /request             → { floor, direction } → Manual hall call
- POST   /destination         → { elevatorId, destinationFloor } → Passenger selects floor

Swagger UI available at: /swagger/index.html

9. Testing Strategy
- xUnit project with unit tests
- Tests cover:
  - RequestElevatorCommandHandler assigns nearest elevator
  - SelectDestinationCommandHandler adds destination correctly
  - GetElevatorStatusQuery returns accurate data
- Uses Moq for repository mocking

10. Deployment and Running Instructions
Prerequisites: .NET 8 SDK

Run:
dotnet run --project src/ElevatorControlSystem.Api

Features on startup:
- API listening (usually http://localhost:5000 or random port)
- Background simulation starts immediately
- Console logs elevator positions and events
- Swagger UI at http://localhost:<port>/swagger/index.html

