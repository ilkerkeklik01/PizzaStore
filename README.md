# PizzaStore - Clean Architecture .NET 10 API

A full-featured .NET 10 Web API built with Clean Architecture principles, demonstrating authentication, authorization, and best practices.

## 🏗️ Architecture

This solution follows **Clean Architecture** with the following layers:

### Projects Structure

```
PizzaStore/
├── src/
│   ├── PizzaStore.Domain/          # Core business entities and interfaces
│   ├── PizzaStore.Application/     # Business logic, MediatR handlers, DTOs
│   ├── PizzaStore.Infrastructure/  # EF Core, Identity, Repositories, Services
│   └── PizzaStore.API/            # Controllers, Middleware, Configuration
└── tests/
    └── PizzaStore.Tests/           # Unit and Integration Tests
```

## 🔑 Features

- ✅ **Clean Architecture** - Separation of concerns with proper dependency flow
- ✅ **ASP.NET Core Identity** - Full authentication system with PBKDF2 password hashing
- ✅ **JWT Bearer Authentication** - Stateless authentication with JWT tokens
- ✅ **Role-based Authorization** - User and Admin roles
- ✅ **MediatR (CQRS Pattern)** - Command/Query separation
- ✅ **Repository + Unit of Work** - Data access abstraction with transaction support
- ✅ **Global Exception Handling** - Centralized error handling middleware
- ✅ **FluentValidation** - Input validation
- ✅ **EF Core In-Memory Database** - For development and testing
- ✅ **Swagger/OpenAPI** - Interactive API documentation
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
- Depends only on Domain
- Contains business logic, DTOs, commands/queries
- MediatR handlers, validators, interfaces

**Infrastructure**
- Depends on Application and Domain
- Implementation of repositories, services
- EF Core, Identity, JWT token generation

**API (Presentation)**
- Depends on Application and Infrastructure
- Controllers, middleware, configuration
- Entry point of the application

### Design Patterns

- **CQRS:** Commands and Queries separated via MediatR
- **Repository Pattern:** Abstraction over data access
- **Unit of Work:** Transaction management
- **Dependency Injection:** Loose coupling
- **Middleware Pattern:** Global exception handling

## 🛠️ Technologies

- .NET 10
- ASP.NET Core Web API
- ASP.NET Core Identity
- Entity Framework Core (In-Memory)
- MediatR
- FluentValidation
- JWT Bearer Authentication
- Swashbuckle (Swagger/OpenAPI)
- DotNetEnv

## 📚 Next Steps

To extend this application:

1. **Add Pizza Entities**
   - Create `Pizza`, `Order`, `OrderItem` entities in Domain
   - Add corresponding repositories and commands/queries

2. **Switch to Real Database**
   - Replace In-Memory with SQL Server/PostgreSQL
   - Add migrations

3. **Add Refresh Tokens**
   - Implement token refresh mechanism
   - Add refresh token endpoint

4. **Add Unit Tests**
   - Test MediatR handlers
   - Test repositories with in-memory database

5. **Add Logging**
   - Integrate Serilog or NLog
   - Log to file/database

## 📄 License

This is a demonstration project for learning purposes.

## 👨‍💻 Author

Built with Clean Architecture best practices and industry-standard security patterns.
