# SocialMediaSolution

A **Social Media Platform** built with **ASP.NET Core MVC** following **Clean Architecture** principles.

## 🏗️ Architecture

This project follows Clean Architecture with four layers:

```
SocialMedia.Web          → Presentation (MVC Controllers, Views, SignalR Hubs)
SocialMedia.Infrastructure → Data Access (EF Core, Repositories)
SocialMedia.Application    → Business Logic (Services, DTOs, Interfaces)
SocialMedia.Domain         → Core Domain (Entities, Enums, Value Objects)
```

### Dependency Rules

| Project | Depends On |
|---|---|
| **Domain** | None (pure domain) |
| **Application** | Domain |
| **Infrastructure** | Application + Domain |
| **Web** | Application + Infrastructure + Domain |

## 📁 Folder Structure

```
SocialMediaSolution/
│
├── SocialMedia.Domain/
│   ├── Entities/
│   ├── Enums/
│   ├── ValueObjects/
│   └── Common/            ← BaseEntity.cs
│
├── SocialMedia.Application/
│   ├── DTOs/
│   ├── Interfaces/         ← IRepository.cs
│   ├── Services/
│   ├── Mapping/
│   └── Validators/
│
├── SocialMedia.Infrastructure/
│   ├── Data/               ← AppDbContext.cs
│   ├── Repositories/
│   ├── Identity/
│   ├── SignalR/
│   └── Services/
│
└── SocialMedia.Web/
    ├── Controllers/
    ├── ViewModels/
    ├── Views/
    │   ├── Account/
    │   ├── Profile/
    │   ├── Posts/
    │   └── Chat/
    ├── Hubs/               ← ChatHub.cs, NotificationHub.cs
    ├── wwwroot/
    │   ├── css/
    │   ├── js/
    │   └── images/
    ├── Extensions/
    └── Middleware/
```

## 🛠️ Tech Stack

| Technology | Purpose |
|---|---|
| ASP.NET Core MVC (.NET 8) | Web framework |
| Entity Framework Core | ORM / Data Access |
| AutoMapper | DTO ↔ Entity mapping |
| FluentValidation | Input validation |
| SignalR | Real-time communication |

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB or full instance)
