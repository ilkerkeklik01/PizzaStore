# 🍕 PizzaStore - Clean Architecture .NET 10 API

A complete, production-ready .NET 10 Web API for a pizza ordering system built with **Clean Architecture** and **Vertical Slice Architecture** principles, featuring authentication, authorization, CQRS with MediatR, and enterprise-grade best practices.

## 📋 Table of Contents
- [Overview](#-overview)
- [Features](#-features)
- [Architecture](#-architecture)
- [Technology Stack](#-technology-stack)
- [Getting Started](#-getting-started)
- [API Endpoints](#-api-endpoints)
- [Authentication & Authorization](#-authentication--authorization)
- [Seed Data](#-seed-data)
- [Testing](#-testing)
- [Project Structure](#-project-structure)
- [Design Patterns](#-design-patterns--principles)

## 🌟 Overview

PizzaStore is a comprehensive pizza ordering API with full CRUD operations for pizzas, toppings, shopping cart management, and order processing. The application demonstrates industry-standard practices with 33+ command/query handlers, 6 controllers, and 31 API endpoints.

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

## 🎯 Features

### Domain Features
- ✅ **Pizza Management** - Full CRUD operations with variants (Small, Medium, Large, ExtraLarge)
- ✅ **Topping Management** - Create, update, delete toppings with pricing
- ✅ **Shopping Cart** - Add pizzas with toppings, update quantities, manage cart items
- ✅ **Order Processing** - Checkout cart, view order history, cancel orders
- ✅ **User Management** - Registration, login, profile management
- ✅ **Admin Dashboard** - User management, order tracking, status updates

### Architecture & Design
- ✅ **Clean Architecture** - Separation of concerns with proper dependency flow
- ✅ **Vertical Slice Architecture** - Feature-based organization (all related components grouped together)
- ✅ **CQRS Pattern** - Complete separation of commands (write) and queries (read) using MediatR
- ✅ **33+ Handlers** - Command and query handlers for all operations
- ✅ **SOLID Principles** - Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
- ✅ **DRY Principle** - Don't Repeat Yourself - modular and reusable components
- ✅ **Modular Design** - Separate Core projects for Auth, CrossCuttingConcerns, and Persistence
- ✅ **Individual Test Projects** - One test project per module for better isolation
- ✅ **DI Extension Pattern** - Each project registers its own services via extension methods

### Technical Features
- ✅ **ASP.NET Core Identity** - Full authentication system with PBKDF2 password hashing
- ✅ **JWT Bearer Authentication** - Stateless authentication with JWT tokens
- ✅ **Role-based Authorization** - User and Admin roles with endpoint protection
- ✅ **MediatR (CQRS Pattern)** - Command/Query separation with 33+ handlers
- ✅ **Repository + Unit of Work** - Data access abstraction with transaction support
- ✅ **Global Exception Handling** - Centralized error handling middleware
- ✅ **FluentValidation** - Input validation with per-feature validators
- ✅ **EF Core In-Memory Database** - For development and testing
- ✅ **Swagger/OpenAPI** - Interactive API documentation with JWT Bearer support
- ✅ **.env Configuration** - Secure configuration management
- ✅ **Soft Deletes** - Data retention with IsDeleted flag
- ✅ **Auditing** - CreatedAt, UpdatedAt timestamps on all entities

## 🛠️ Technology Stack

- **.NET 10** - Latest .NET framework
- **ASP.NET Core Web API** - RESTful API framework
- **ASP.NET Core Identity** - Authentication and user management
- **Entity Framework Core 10** (In-Memory) - ORM and data access
- **MediatR 12.4.1** - CQRS implementation with pipeline behaviors
- **FluentValidation 11.11.0** - Input validation
- **JWT Bearer Authentication** - Stateless token-based auth
- **Swashbuckle 9.0.6** (Swagger/OpenAPI) - API documentation
- **DotNetEnv 3.1.1** - Environment variable management
- **xUnit** - Testing framework (ready for test implementation)

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

The application seeds two default users on startup:

### Admin User
- **Email:** `admin@pizzastore.com`
- **Password:** `Admin123`
- **Role:** Admin
- **Access:** All endpoints including admin-only features

### Regular User
- **Email:** `user@pizzastore.com`
- **Password:** `User123`
- **Role:** User
- **Access:** Public and authenticated user endpoints

## 🔐 API Endpoints

The API provides **31 endpoints** across **6 controllers**:

### 1. Authentication Controller (`/api/auth`)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/register` | Register a new user | Public |
| POST | `/login` | Login and receive JWT token | Public |
| GET | `/me` | Get current user information | Authenticated |

### 2. Pizza Controller (`/api/pizza`)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/` | Get all pizzas | Public |
| GET | `/{id}` | Get pizza by ID | Public |
| GET | `/type/{type}` | Get pizzas by type | Public |
| POST | `/` | Create pizza with variants | Admin |
| PUT | `/{id}` | Update pizza | Admin |
| DELETE | `/{id}` | Soft delete pizza | Admin |
| POST | `/{id}/variants` | Add variant to pizza | Admin |
| PUT | `/{pizzaId}/variants/{variantId}` | Update pizza variant | Admin |
| DELETE | `/{pizzaId}/variants/{variantId}` | Delete pizza variant | Admin |

**Pizza Types:** Vegetarian, MeatLovers, Hawaiian, Veggie, Custom, Supreme, Margherita  
**Pizza Sizes:** Small, Medium, Large, ExtraLarge

### 3. Topping Controller (`/api/topping`)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/` | Get all toppings | Public |
| POST | `/` | Create new topping | Admin |
| PUT | `/{id}` | Update topping | Admin |
| DELETE | `/{id}` | Soft delete topping | Admin |

### 4. Cart Controller (`/api/cart`)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/` | Get user's cart | Authenticated |
| POST | `/items` | Add pizza to cart | Authenticated |
| GET | `/items/{cartItemId}` | Get cart item | Authenticated |
| PUT | `/items/{cartItemId}` | Update cart item quantity | Authenticated |
| PATCH | `/items/{cartItemId}/increase` | Increase quantity by 1 | Authenticated |
| PATCH | `/items/{cartItemId}/decrease` | Decrease quantity by 1 | Authenticated |
| DELETE | `/items/{cartItemId}` | Remove item from cart | Authenticated |
| DELETE | `/` | Clear entire cart | Authenticated |

### 5. Order Controller (`/api/order`)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/checkout` | Checkout cart & create order | Authenticated |
| GET | `/` | Get user's orders | Authenticated |
| GET | `/{id}` | Get order by ID | Authenticated |
| POST | `/{id}/cancel` | Cancel order | Authenticated |

### 6. Admin Controller (`/api/admin`)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/users` | Get all users | Admin |
| GET | `/users/{id}` | Get user by ID | Admin |
| GET | `/users/{id}/orders` | Get orders by user ID | Admin |
| GET | `/orders` | Get all orders (with filters) | Admin |
| PUT | `/orders/{id}/status` | Update order status | Admin |

**Order Statuses:** Pending, Confirmed, Preparing, OutForDelivery, Delivered, Cancelled

**Admin Orders Filters:**
- `?status=Pending` - Filter by order status
- `?userId={id}` - Filter by user
- `?fromDate=2024-01-01&toDate=2024-12-31` - Filter by date range
- Multiple filters can be combined

## 🔒 Authentication & Authorization

### How to Use the API

1. **Login:** POST to `/api/auth/login` with email and password
2. **Get Token:** Copy the JWT token from the response
3. **Authorize:** Add header `Authorization: Bearer {token}` to authenticated requests
4. **Access:** User role can access user endpoints, Admin role can access all endpoints

### Security Features

#### Password Security
- **Algorithm:** PBKDF2 with HMAC-SHA256
- **Iterations:** 10,000
- **Salt:** 128-bit (auto-generated per user)
- **Key Length:** 256-bit

#### JWT Configuration
- Tokens expire after configured minutes (default: 60)
- Includes user ID, email, and roles as claims
- Signed with HMAC-SHA256
- Configured via `.env` file

### Testing with Swagger

1. Start the API and navigate to Swagger UI
2. Use `/api/auth/login` endpoint with default credentials
3. Copy the JWT token from response
4. Click **"Authorize"** button at top of Swagger UI
5. Enter the token (without "Bearer" prefix)
6. Click "Authorize"
7. Try authenticated endpoints

### Testing with HTTP File

The `PizzaStore.API.http` file contains all 31 endpoints with sample requests:
1. Login using endpoint 1.2 or 1.3
2. Copy the JWT token from response
3. Update the `@token` variable at the top of the file
4. Execute any authenticated request

## 🌱 Seed Data

The application automatically seeds data on startup:

### Roles
- Admin
- User

### Users
- **Admin:** admin@pizzastore.com / Admin123
- **User:** user@pizzastore.com / User123

### Toppings (10 items)
| Name | Price |
|------|-------|
| Pepperoni | $1.50 |
| Mushrooms | $1.00 |
| Onions | $0.75 |
| Bell Peppers | $1.00 |
| Black Olives | $1.25 |
| Extra Cheese | $2.00 |
| Bacon | $1.75 |
| Sausage | $1.50 |
| Pineapple | $1.00 |
| Jalapeños | $0.75 |

### Pizzas (5 items with 4 size variants each)

**1. Margherita**
- Description: Classic pizza with fresh mozzarella, tomatoes, and basil
- Type: Margherita
- Prices: Small $8.99 | Medium $12.99 | Large $16.99 | XL $20.99

**2. Pepperoni**
- Description: Classic pepperoni pizza with mozzarella cheese and tomato sauce
- Type: MeatLovers
- Prices: Small $9.99 | Medium $13.99 | Large $17.99 | XL $21.99

**3. Hawaiian**
- Description: Ham, pineapple, and mozzarella cheese
- Type: Hawaiian
- Prices: Small $10.99 | Medium $14.99 | Large $18.99 | XL $22.99

**4. Veggie Supreme**
- Description: Loaded with mushrooms, onions, bell peppers, olives, and tomatoes
- Type: Vegetarian
- Prices: Small $10.49 | Medium $14.49 | Large $18.49 | XL $22.49

**5. Meat Lovers**
- Description: Loaded with pepperoni, sausage, bacon, and ham
- Type: MeatLovers
- Prices: Small $11.99 | Medium $15.99 | Large $19.99 | XL $24.99

## 📐 Architecture Principles

### Clean Architecture Layers

**Domain (Core)**
- No dependencies on other layers
- Contains entities and core business interfaces
- Entities: `Pizza`, `PizzaVariant`, `Topping`, `Cart`, `CartItem`, `Order`, `OrderItem`, `ApplicationUser`, `ApplicationRole`
- Enums: `PizzaType`, `PizzaSize`, `OrderStatus`
- Interfaces: `IRepository<T>`, `IUnitOfWork`

**Application**
- Depends only on Domain
- Contains business logic organized by features (Vertical Slice Architecture)
- **33+ Handlers** implementing CQRS pattern
- Feature structure: `Features/{Commands|Queries}/{Entity}/{Action}/`
- Each feature contains: Command/Query, Handler, DTO, Validator

**Handlers Overview:**
```
Commands (Write Operations):
├── Auth
│   ├── Register (RegisterUserCommand)
│   └── Login (LoginUserCommand)
├── Pizza
│   ├── CreatePizza
│   ├── UpdatePizza
│   └── DeletePizza
├── PizzaVariant
│   ├── AddPizzaVariant
│   ├── UpdatePizzaVariant
│   └── DeletePizzaVariant
├── Topping
│   ├── CreateTopping
│   ├── UpdateTopping
│   └── DeleteTopping
├── Cart
│   ├── AddPizzaToCart
│   ├── UpdateCartItemQuantity
│   ├── IncreaseCartItemQuantity
│   ├── DecreaseCartItemQuantity
│   ├── RemoveCartItem
│   └── ClearCart
├── Order
│   ├── CheckoutCart
│   └── CancelOrder
└── Admin
    └── UpdateOrderStatus

Queries (Read Operations):
├── Pizza
│   ├── GetAllPizzas
│   ├── GetPizzaById
│   └── GetPizzasByType
├── Topping
│   └── GetAllToppings
├── Cart
│   ├── GetUserCart
│   └── GetCartItem
├── Order
│   ├── GetMyOrders
│   └── GetOrderById
└── Admin
    ├── GetAllUsers
    ├── GetUserById
    ├── GetOrdersByUserId
    └── GetAllOrders
```

**Core.Auth**
- Depends on Domain and Core.CrossCuttingConcerns
- Authentication and authorization services
- JWT token generation and validation
- Auth-related DTOs and interfaces
- Services: `AuthService`, `JwtTokenGenerator`, `CurrentUserService`
- Self-contained with its own DI registration

**Core.CrossCuttingConcerns**
- No business dependencies
- Global exception handling middleware
- Custom exception types: `NotFoundException`, `ValidationException`, `UnauthorizedException`
- Extensible logging infrastructure
- Self-contained with its own DI registration

**Infrastructure.Persistence**
- Depends on Domain
- EF Core DbContext with ASP.NET Core Identity integration
- Repository pattern implementation
- Unit of Work for transaction management
- Database initialization and seeding (DbInitializer)
- Self-contained with its own DI registration

**API (Presentation)**
- Depends on all other layers
- **6 Controllers:** Auth, Pizza, Topping, Cart, Order, Admin
- **31 Endpoints** with proper HTTP verbs and status codes
- Swagger/OpenAPI configuration with JWT support
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
- **Entity Framework Core 10** (In-Memory) - ORM and data access
- **MediatR 12.4.1** - CQRS implementation with pipeline behaviors
- **FluentValidation 11.11.0** - Input validation
- **JWT Bearer Authentication** - Stateless token-based auth
- **Swashbuckle 9.0.6** (Swagger/OpenAPI) - API documentation
- **DotNetEnv 3.1.1** - Environment variable management
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
   - Add XML documentation comments

5. **Write Tests**
   - Add tests in appropriate test project
   - Follow AAA pattern (Arrange, Act, Assert)

## 🚀 Next Steps & Enhancements

### Database
1. **Switch to Real Database**
   - Replace In-Memory with SQL Server/PostgreSQL
   - Add EF Core migrations
   - Update `PersistenceServiceExtensions.cs`

### Authentication
2. **Add Refresh Tokens**
   - Extend `Core.Auth` with refresh token service
   - Add token refresh endpoint in AuthController
   - Store refresh tokens in database

3. **Add Email Confirmation**
   - Implement email service
   - Add email verification flow
   - Send confirmation emails on registration

### Features
4. **Payment Integration**
   - Add payment domain entities
   - Integrate Stripe/PayPal
   - Implement payment processing commands

5. **Real-time Updates**
   - Add SignalR for order status updates
   - Implement notifications hub
   - Real-time cart updates

6. **File Upload**
   - Add image upload for pizzas
   - Store images in blob storage (Azure/AWS S3)
   - Generate thumbnails

### Testing & Quality
7. **Comprehensive Testing**
   - Unit tests for handlers with mocked dependencies
   - Integration tests with TestServer
   - Repository tests with in-memory database
   - Validator tests with FluentValidation test extensions
   - Achieve >80% code coverage

8. **Advanced Logging**
   - Integrate Serilog or NLog
   - Add structured logging
   - Log to file/database/cloud (Seq, Application Insights)

### Performance & Scalability
9. **Add Caching**
   - Implement Redis distributed cache
   - Cache frequently accessed data (pizzas, toppings)
   - Add cache invalidation strategies

10. **API Versioning**
    - Implement versioning strategy (URL/header/media type)
    - Version controllers appropriately
    - Maintain backward compatibility

11. **Rate Limiting**
    - Add rate limiting middleware
    - Protect against abuse
    - Configure per-endpoint limits

## 📄 License

This is a demonstration project for learning purposes.

## 📖 Additional Documentation

- **QUICK_REFERENCE.md** - Quick reference for common operations
- **CART_FEATURES_SUMMARY.md** - Detailed cart feature documentation
- **CART_INDEX.md** - Cart implementation index
- **IMPLEMENTATION_DETAILS.md** - Technical implementation details
- **CHANGELOG.md** - Version history and changes
- **PizzaStore.API.http** - Complete HTTP request collection for all 31 endpoints

## 👨‍💻 Author

Built with Clean Architecture best practices, CQRS pattern, and industry-standard security patterns.
