# PizzaStore - Clean Architecture .NET 10 API

A full-featured .NET 10 Web API built with **Clean Architecture** and **Vertical Slice Architecture** principles, demonstrating authentication, authorization, SOLID principles, and enterprise-grade best practices.

## 🏗️ Architecture

This solution follows **Clean Architecture** with enhanced separation of concerns and modular design:

### Projects Structure

```
PizzaStore/
├── src/
│   ├── PizzaStore.Domain/                           # Core business entities and interfaces
│   ├── PizzaStore.Application/                      # Business logic (Feature-based organization)
│   │   └── Features/
│   │       ├── Commands/                            # Write operations
│   │       │   └── Auth/
│   │       │       ├── Register/                    # Feature: User Registration
│   │       │       │   ├── RegisterUserCommand.cs
│   │       │       │   ├── RegisterUserCommandHandler.cs
│   │       │       │   ├── RegisterUserDto.cs
│   │       │       │   └── RegisterUserDtoValidator.cs
│   │       │       └── Login/                       # Feature: User Login
│   │       └── Queries/                             # Read operations
│   │           └── Users/
│   ├── PizzaStore.Core.Auth/                        # Authentication & Authorization
│   │   ├── Services/                                # AuthService, JwtTokenGenerator
│   │   ├── DTOs/                                    # Auth-related DTOs
│   │   ├── Interfaces/                              # IAuthService, IJwtTokenGenerator
│   │   └── Extensions/                              # DI registration
│   ├── PizzaStore.Core.CrossCuttingConcerns/        # Cross-cutting concerns
│   │   ├── Middleware/                              # Global exception handling
│   │   ├── Exceptions/                              # Custom exceptions
│   │   └── Extensions/                              # DI registration
│   ├── PizzaStore.Infrastructure.Persistence/       # Data access layer
│   │   ├── Data/                                    # DbContext, DbInitializer
│   │   ├── Repositories/                            # Repository implementations
│   │   └── Extensions/                              # DI registration
│   └── PizzaStore.API/                              # Controllers, Configuration
└── tests/
    ├── PizzaStore.API.Tests/
    ├── PizzaStore.Application.Tests/
    ├── PizzaStore.Domain.Tests/
    ├── PizzaStore.Core.Auth.Tests/
    ├── PizzaStore.Core.CrossCuttingConcerns.Tests/
    └── PizzaStore.Infrastructure.Persistence.Tests/
```

## 🔑 Features

### Architecture & Design
- ✅ **Clean Architecture** - Separation of concerns with proper dependency flow
- ✅ **Vertical Slice Architecture** - Feature-based organization (all related components grouped together)
- ✅ **SOLID Principles** - Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
- ✅ **DRY Principle** - Don't Repeat Yourself - modular and reusable components
- ✅ **Modular Design** - Separate Core projects for Auth, CrossCuttingConcerns, and Persistence
- ✅ **Individual Test Projects** - One test project per module for better isolation
- ✅ **DI Extension Pattern** - Each project registers its own services via extension methods

### Technical Features
- ✅ **ASP.NET Core Identity** - Full authentication system with PBKDF2 password hashing
- ✅ **JWT Bearer Authentication** - Stateless authentication with JWT tokens
- ✅ **Role-based Authorization** - User and Admin roles
- ✅ **MediatR (CQRS Pattern)** - Command/Query separation with handlers
- ✅ **Repository + Unit of Work** - Data access abstraction with transaction support
- ✅ **Global Exception Handling** - Centralized error handling middleware
- ✅ **FluentValidation** - Input validation with per-feature validators
- ✅ **EF Core In-Memory Database** - For development and testing
- ✅ **Swagger/OpenAPI** - Interactive API documentation with JWT Bearer support
- ✅ **.env Configuration** - Secure configuration management

## 🚀 Getting Started

### Prerequisites

- .NET 10 SDK
- Your favorite IDE (Visual Studio, VS Code, Rider)

### Setup

1. **Clone and navigate to the project**
   ```bash
   cd PizzaStore
   ```

2. **Create .env file in the solution root**
   ```bash
   cp .env.example .env
   ```

3. **Update .env with your settings**
   ```
   JWT_SECRET_KEY=YourVerySecureSecretKeyThatIsAtLeast32CharactersLong!
   JWT_ISSUER=PizzaStoreAPI
   JWT_AUDIENCE=PizzaStoreClients
   JWT_EXPIRY_MINUTES=60
   ```

4. **Restore dependencies**
   ```bash
   dotnet restore
   ```

5. **Build the solution**
   ```bash
   dotnet build
   ```

6. **Run the API**
   ```bash
   cd src/PizzaStore.API
   dotnet run
   ```

7. **Access Swagger UI**
   - Open your browser to `https://localhost:5001/swagger` (or the port shown in console)

## 📝 Default Users

The application seeds two default users:

### Admin User
- **Email:** admin@pizzastore.com
- **Password:** Admin123
- **Role:** Admin

### Regular User
- **Email:** user@pizzastore.com
- **Password:** User123
- **Role:** User

## 🔐 API Endpoints

### Authentication

#### Register
```http
POST /api/auth/register
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "password": "Password123"
}
```

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "john.doe@example.com",
  "password": "Password123"
}
```

**Response:**
```json
{
  "token": "eyJhbGc...",
  "user": {
    "id": "...",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com"
  }
}
```

#### Get Current User
```http
GET /api/auth/me
Authorization: Bearer {your-jwt-token}
```

### Pizza Endpoints

#### Get All Pizzas (Authenticated)
```http
GET /api/pizza
Authorization: Bearer {your-jwt-token}
```

#### Get Admin Data (Admin Only)
```http
GET /api/pizza/admin
Authorization: Bearer {your-jwt-token}
```

## 🔒 Security Features

### Password Security
- **Algorithm:** PBKDF2 with HMAC-SHA256
- **Iterations:** 10,000
- **Salt:** 128-bit (auto-generated per user)
- **Key Length:** 256-bit

### JWT Configuration
- Tokens expire after configured minutes (default: 60)
- Includes user ID, email, and roles as claims
- Signed with HMAC-SHA256

### Environment Variables
- All sensitive data stored in `.env` file
- `.env` excluded from version control
- Template provided in `.env.example`

## 🧪 Testing with Swagger

1. Start the API
2. Navigate to Swagger UI
3. Use the `/api/auth/login` endpoint with a default user
4. Copy the JWT token from the response
5. Click "Authorize" button at the top
6. Enter: `Bearer {your-token}`
7. Try authenticated endpoints

## 📐 Architecture Principles

### Clean Architecture Layers

**Domain (Core)**
- No dependencies on other layers
- Contains entities and core business interfaces
- `ApplicationUser`, `ApplicationRole`, `IRepository`, `IUnitOfWork`

**Application**
- Depends only on Domain and Core.Auth
- Contains business logic organized by features
- Feature-based structure: `Features/Commands/Auth/Register/`
- Each feature contains: Command, Handler, DTO, Validator (vertical slice)
- MediatR CQRS pattern implementation

**Core.Auth**
- Depends on Domain and Core.CrossCuttingConcerns
- Authentication and authorization services
- JWT token generation
- Auth-related DTOs and interfaces
- Self-contained with its own DI registration

**Core.CrossCuttingConcerns**
- No business dependencies
- Global exception handling middleware
- Custom exception types
- Logging infrastructure (extensible)
- Self-contained with its own DI registration

**Infrastructure.Persistence**
- Depends on Domain
- EF Core DbContext, repositories, data seeding
- Database configuration and migrations
- Unit of Work implementation
- Self-contained with its own DI registration

**API (Presentation)**
- Depends on Application, Core.Auth, Core.CrossCuttingConcerns, Infrastructure.Persistence
- Controllers, Swagger configuration
- Entry point and composition root
- Uses extension methods from all Core/Infrastructure projects

### Design Patterns & Principles

- **CQRS:** Commands and Queries separated via MediatR
- **Vertical Slice Architecture:** All components for a feature grouped together
- **Repository Pattern:** Abstraction over data access
- **Unit of Work:** Transaction management
- **Dependency Injection:** Loose coupling, extension methods per project
- **Middleware Pattern:** Global exception handling
- **SOLID:**
  - **S**ingle Responsibility: Each class has one reason to change
  - **O**pen/Closed: Open for extension, closed for modification
  - **L**iskov Substitution: Interfaces follow contracts
  - **I**nterface Segregation: Small, focused interfaces
  - **D**ependency Inversion: Depend on abstractions, not concretions

### Naming Conventions

- **Projects:** `PizzaStore.{Layer}.{Module}` (e.g., `PizzaStore.Core.Auth`)
- **Namespaces:** Match folder structure (e.g., `PizzaStore.Application.Features.Commands.Auth.Register`)
- **Features:** `{Action}{Entity}{Type}` (e.g., `RegisterUserCommand`, `LoginUserCommandHandler`)
- **DTOs:** `{Action}{Entity}Dto` (e.g., `RegisterUserDto`, `AuthResponseDto`)
- **Validators:** `{Dto}Validator` (e.g., `RegisterUserDtoValidator`)
- **Extensions:** `{Module}ServiceExtensions` (e.g., `AuthServiceExtensions`)
- **Interfaces:** `I{Name}` prefix (e.g., `IAuthService`, `IRepository<T>`)

## 🛠️ Technologies

- **.NET 10** - Latest .NET framework
- **ASP.NET Core Web API** - RESTful API framework
- **ASP.NET Core Identity** - Authentication and user management
- **Entity Framework Core** (In-Memory) - ORM and data access
- **MediatR** - CQRS implementation
- **FluentValidation** - Input validation
- **JWT Bearer Authentication** - Stateless token-based auth
- **Swashbuckle 9.0.6** (Swagger/OpenAPI) - API documentation
- **DotNetEnv** - Environment variable management
- **xUnit** - Testing framework (ready for test implementation)

## 🧪 Testing

The solution includes 6 individual test projects (one per module):
- `PizzaStore.API.Tests` - API layer tests
- `PizzaStore.Application.Tests` - Business logic and handler tests
- `PizzaStore.Domain.Tests` - Domain entity tests
- `PizzaStore.Core.Auth.Tests` - Authentication service tests
- `PizzaStore.Core.CrossCuttingConcerns.Tests` - Middleware and exception tests
- `PizzaStore.Infrastructure.Persistence.Tests` - Repository and DbContext tests

```bash
# Run all tests
dotnet test

# Run tests for a specific project
dotnet test tests/PizzaStore.Application.Tests
```

## 📚 Adding New Features

To add a new feature following the established architecture:

1. **Create Feature Folder Structure**
   ```
   src/PizzaStore.Application/Features/Commands/Pizza/CreatePizza/
   ├── CreatePizzaCommand.cs
   ├── CreatePizzaCommandHandler.cs
   ├── CreatePizzaDto.cs
   └── CreatePizzaDtoValidator.cs
   ```

2. **Add Domain Entity** (if needed)
   - Create entity in `src/PizzaStore.Domain/Entities/`
   - Add repository interface in `src/PizzaStore.Domain/Interfaces/`

3. **Implement Repository** (if needed)
   - Create repository in `src/PizzaStore.Infrastructure.Persistence/Repositories/`
   - Register in `PersistenceServiceExtensions.cs`

4. **Add Controller Endpoint**
   - Create/update controller in `src/PizzaStore.API/Controllers/`
   - Use MediatR to send commands/queries

5. **Write Tests**
   - Add tests in appropriate test project
   - Follow AAA pattern (Arrange, Act, Assert)

## 🚀 Next Steps

To extend this application:

1. **Add Pizza Domain**
   - Create `Pizza`, `Order`, `OrderItem` entities in Domain
   - Add corresponding repositories and feature folders
   - Implement CRUD operations

2. **Switch to Real Database**
   - Replace In-Memory with SQL Server/PostgreSQL
   - Add EF Core migrations
   - Update `PersistenceServiceExtensions.cs`

3. **Add Refresh Tokens**
   - Extend `Core.Auth` with refresh token service
   - Add token refresh endpoint in AuthController
   - Store refresh tokens in database

4. **Implement Unit Tests**
   - Test MediatR handlers with mocked dependencies
   - Test repositories with in-memory database
   - Test validators with FluentValidation test extensions

5. **Add Advanced Logging**
   - Integrate Serilog or NLog in `Core.CrossCuttingConcerns`
   - Add structured logging
   - Log to file/database/cloud

6. **Add API Versioning**
   - Implement versioning strategy (URL/header/media type)
   - Version controllers appropriately

7. **Add Caching**
   - Implement Redis distributed cache
   - Add caching middleware in `Core.CrossCuttingConcerns`

## 📄 License

This is a demonstration project for learning purposes.

## 👨‍💻 Author

Built with Clean Architecture best practices and industry-standard security patterns.
