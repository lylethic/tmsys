# 🚀 TMS - Task Management System

[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-blue)](https://www.postgresql.org/)
[![SignalR](https://img.shields.io/badge/SignalR-RealTime-green)](https://dotnet.microsoft.com/apps/aspnet/signalr)

## 📋 Introduction

**TMS (Task Management System)** is a professional internal task management system for IT teams. It is built on **.NET 8.0** with **Clean Architecture** to ensure scalability, maintainability, and high performance.

### ✨ Key Features

#### 🔐 Authentication & Authorization

- **JWT Authentication**: Issue Access and Refresh Tokens for users
- **Role-Based Access Control (RBAC)**: Fine-grained role permissions
- **Permission Management**: Flexible permission handling per function
- **OTP Verification**: Two-factor verification via email OTP

#### 📊 Project & Task Management

- **Project Management**: Create and manage multiple project types
- **Task Management**: Assign and track task progress in detail
- **Task Assignment**: Assign tasks to multiple members
- **Progress Tracking**: Real-time status updates and monitoring
- **Approval Workflow**: Multi-level task approval process

#### 🔔 Notification System

- **Real-time Notifications**: Instant alerts via SignalR
- **Multi-channel Notifications**: Email and in-app delivery
- **Notification Categories**: Prioritized notification grouping
- **Background Worker**: Automated notification handling with Hangfire

#### 📈 Reporting & Analytics

- **Report Generation**: Build project and task reports
- **Statistics Dashboard**: Overview of productivity metrics
- **Export to Excel**: Export reports to Excel with ClosedXML

#### 👥 User Management

- **User Management**: Manage user profiles and info
- **Role Assignment**: Assign roles to users
- **User Activity Tracking**: Track user actions

#### 📁 File & Media Management

- **Cloudinary Integration**: Upload and manage images/files on the cloud
- **File Storage**: Store uploads locally
- **Media Asset Management**: Manage project media assets

#### 🔄 Background Jobs

- **Hangfire Integration**: Manage background jobs
- **Scheduled Tasks**: Automate recurring jobs
- **Job Dashboard**: Monitor jobs via UI

---

## 🏗️ System Architecture

### Clean Architecture Structure

```
tms_server/
├── 📁 Application/              # Business Logic Layer
│   ├── DTOs/                    # Data Transfer Objects
│   ├── Models/                  # View Models
│   ├── Request/                 # Request Models
│   └── Common/                  # Shared Application Logic
│
├── 📁 Domain/                   # Core Domain Layer
│   ├── Entities/                # Domain Entities
│   └── AppDataContext/          # Database Context
│
├── 📁 Repositories/             # Data Access Layer
│   ├── *Repository.cs           # Repository Pattern Implementation
│   └── SeedDataService.cs       # Database Seeding
│
├── 📁 Services/                 # Application Services
│   ├── CloudinaryService.cs     # Cloud Storage Service
│   ├── EmailTemplateManager.cs  # Email Service
│   ├── NotificationService.cs   # Notification Service
│   └── AssistantService.cs      # AI Assistant Service
│
├── 📁 Controllers/              # API Endpoints (Presentation Layer)
│   ├── v1/                      # API Version 1
│   └── AuthController.cs        # Authentication Controller
│
├── 📁 Hubs/                     # SignalR Hubs (Real-time)
│   ├── NotificationHub.cs       # Real-time Notification Hub
│   └── Worker.cs                # Background Worker
│
├── 📁 Hangfire/                 # Background Job Processing
│   ├── IJobRunService.cs        # Job Service Interface
│   └── JobRunService.cs         # Job Service Implementation
│
├── 📁 Common/                   # Shared Infrastructure
│   ├── Middlewares/             # Custom Middlewares
│   ├── Exceptions/              # Custom Exceptions
│   ├── Utils/                   # Utility Classes
│   ├── Constants/               # Application Constants
│   └── Settings/                # Configuration Settings
│
└── 📁 wwwroot/                  # Static Files & Uploads
```

---

## 🛠️ Technologies & Libraries

### Core Framework

- **.NET 8.0** - Primary framework
- **ASP.NET Core Web API** - RESTful API
- **Dapper** - Micro ORM for high performance

### Database

- **PostgreSQL 16** - Primary database
- **Npgsql** - PostgreSQL provider

### Authentication & Security

- **JWT (JSON Web Tokens)** - Authentication
- **BCrypt.Net** - Password hashing

### Real-time Communication

- **SignalR** - WebSocket/Long Polling
- **SignalR Core** - Real-time notifications

### Background Processing

- **Hangfire** - Background job processing
- **Hangfire.PostgreSql** - Hangfire storage

### Cloud & Storage

- **CloudinaryDotNet** - Cloud media storage
- **File System** - Local file storage

### Utilities

- **AutoMapper** - Object mapping
- **DotNetEnv** - Environment variables
- **log4net** - Logging framework
- **Medo.Uuid7** - UUID v7 generation

### API Documentation

- **Swashbuckle (Swagger)** - API documentation
- **API Versioning** - Version management

---

## 🚀 Setup & Run

### System Requirements

- .NET SDK 8.0 or higher
- PostgreSQL 16

### 1️⃣ Clone Repository

```bash
git clone https://github.com/lyle975/tms_server.git
cd tms_server
```

### 2️⃣ Configure Environment Variables

Create a `.env` or `.env.development` file:

### 3️⃣ Run Locally (Development)

```bash
# Run with HTTP
dotnet watch run
# or
dotnet run --environment "Development"

# Run with HTTPS
dotnet run --launch-profile https
```

## 📊 Database Management

### Entity Framework Commands

```bash
# Scaffold database (reverse engineering)
dotnet ef dbcontext scaffold "Host=localhost;Port=5432;Database=tms_server;Username=postgres;Password=111111" Npgsql.EntityFrameworkCore.PostgreSQL --schema public  --output-dir Models --context TMSDbContext --context-dir Data --use-database-names --force
```

---

## 🔑 API Endpoints

### Authentication

```
POST   /api/v1/auths/login           # Login
POST   /api/v1/auths/register        # Register
POST   /api/v1/auths/refresh-token   # Refresh token
POST   /api/v1/auths/verify-otp      # Verify OTP
POST   /api/v1/auths/logout          # Logout
```

### Users

```
GET    /api/v1/users                 # List users
GET    /api/v1/users/{id}            # User detail
POST   /api/v1/users                 # Create user
PUT    /api/v1/users/{id}            # Update user
DELETE /api/v1/users/{id}            # Delete user
```

### Projects

```
GET    /api/v1/projects              # List projects
GET    /api/v1/projects/{id}         # Project detail
POST   /api/v1/projects              # Create project
PUT    /api/v1/projects/{id}         # Update project
DELETE /api/v1/projects/{id}         # Delete project
GET    /api/v1/projects/types        # Project types
```

### Tasks

```
GET    /api/v1/tasks                 # List tasks
GET    /api/v1/tasks/{id}            # Task detail
POST   /api/v1/tasks                 # Create task
PUT    /api/v1/tasks/{id}            # Update task
DELETE /api/v1/tasks/{id}            # Delete task
POST   /api/v1/tasks/{id}/assign     # Assign task
PUT    /api/v1/tasks/{id}/status     # Update status
```

### Notifications

```
GET    /api/v1/notifications         # List notifications
GET    /api/v1/notifications/{id}    # Notification detail
POST   /api/v1/notifications/read    # Mark as read
DELETE /api/v1/notifications/{id}    # Delete notification
```

### Reports & Statistics

```
GET    /api/v1/reports               # List reports
POST   /api/v1/reports               # Create report
GET    /api/v1/reports/export        # Export Excel report
GET    /api/v1/statistics/overview   # Overview statistics
GET    /api/v1/statistics/tasks      # Task statistics
```

_See full list in Swagger UI_

---

## 🔄 SignalR Real-time Events

### Client Methods

```javascript
// Connect
const connection = new signalR.HubConnectionBuilder()
  .withUrl('http://localhost:5000/tms/api/hubs/notifications')
  .build();

// Start connection
await connection.start();

// Receive notification
connection.on('ReceiveMessage', (userId, message) => {
  console.log(`${userId}: ${message}`);
});

// Send message to a specific user
await connection.invoke('SendMessageToUser', targetUserId, message);

// Broadcast to everyone
await connection.invoke('BroadcastMessage', message);
```

### Test SignalR

Open the browser and visit:

- http://localhost:5000/tms/api/test-client.html
- http://localhost:5000/tms/api/chat.html

---

## 📝 Logging

The system uses **log4net** for logging. Configuration is in `log4net.config`.

## 🔒 Security Features

✅ **JWT Authentication** with Access & Refresh Token  
✅ **Password Hashing** with BCrypt  
✅ **Role-Based Authorization**  
✅ **Permission-Based Access Control**  
✅ **Rate Limiting** (5 requests/10 seconds)  
✅ **Request Logging & Monitoring**  
✅ **CORS Configuration**  
✅ **Error Handling Middleware**  
✅ **OTP Verification**  
✅ **HTTPS Support**

---

## 👨‍💻 Development

### Code Structure

- Follow **Clean Architecture** principles
- Use **Repository Pattern** for data access
- Apply **Dependency Injection**
- **Async/Await** for all I/O operations
- **AutoMapper** for object mapping
- **DTOs** to transfer data between layers

### Naming Conventions

- **Controllers**: `{Entity}Controller.cs`
- **Repositories**: `{Entity}Repository.cs`
- **Services**: `{Feature}Service.cs`
- **DTOs**: `{Entity}Dto.cs`
- **Requests**: `{Action}{Entity}Request.cs`

---

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License.

---

## 📞 Contact

- **Repository**: [https://github.com/lyle975/tms_server](https://github.com/lyle975/tms_server)
- **Issues**: [https://github.com/lyle975/tms_server/issues](https://github.com/lyle975/tms_server/issues)

---

**Made with ❤️ by Ly**
