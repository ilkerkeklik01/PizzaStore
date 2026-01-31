# Changelog

All notable changes to the PizzaStore project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2026-01-31

### 🎉 Major Architectural Refactoring

This release represents a complete architectural overhaul of the PizzaStore application to improve maintainability, testability, and separation of concerns while adhering to SOLID and DRY principles.

### ✨ Added

#### New Projects
- **PizzaStore.Core.Auth** - Dedicated authentication and authorization module
  - `AuthService` - User authentication logic
  - `JwtTokenGenerator` - JWT token creation
  - Auth-related DTOs (`RegisterUserDto`, `LoginUserDto`, `AuthResponseDto`, `UserResponseDto`)
  - `AuthServiceExtensions` - DI registration with JWT configuration
  
- **PizzaStore.Core.CrossCuttingConcerns** - Cross-cutting concerns module
  - `GlobalExceptionHandlingMiddleware` - Centralized exception handling
  - Custom exceptions (`ValidationException`, `NotFoundException`, `UnauthorizedException`, `ForbiddenException`)
  - `CrossCuttingServiceExtensions` - DI registration
  
- **PizzaStore.Infrastructure.Persistence** - Data access layer
  - `ApplicationDbContext` - EF Core database context
  - `DbInitializer` - Database seeding logic
  - `UnitOfWork` - Transaction management
  - Repository implementations (`Repository<T>`, `UserRepository`)
  - `PersistenceServiceExtensions` - DI registration with DbContext and Identity setup

#### New Test Projects (6 Individual Test Projects)
- `PizzaStore.API.Tests` - API layer testing
- `PizzaStore.Application.Tests` - Business logic and handler testing
- `PizzaStore.Domain.Tests` - Domain entity testing
- `PizzaStore.Core.Auth.Tests` - Authentication service testing
- `PizzaStore.Core.CrossCuttingConcerns.Tests` - Middleware testing
- `PizzaStore.Infrastructure.Persistence.Tests` - Repository testing

#### Feature-Based Organization
- Implemented **Vertical Slice Architecture** in Application layer
- New structure: `Features/Commands/Auth/Register/` and `Features/Commands/Auth/Login/`
- Each feature folder contains:
  - Command class
  - Command handler
  - DTO
  - Validator
  
### 🔄 Changed

#### Architecture
- **From:** Monolithic Infrastructure project with all implementations
- **To:** Modular Core projects with clear separation of concerns
- **From:** Type-based organization (Commands/, Queries/, DTOs/ folders)
- **To:** Feature-based organization (Features/Commands/Auth/Register/)

#### Application Layer
- Reorganized from type-based to feature-based structure
- Moved `RegisterUserCommand` and related files to `Features/Commands/Auth/Register/`
- Moved `LoginUserCommand` and related files to `Features/Commands/Auth/Login/`
- Updated all namespaces to follow new structure pattern

#### Dependency Flow
- Resolved circular dependency by moving Auth DTOs to `Core.Auth.DTOs`
- Application now only depends on Domain and Core.Auth
- Each Core/Infrastructure project is self-contained with its own DI registration

#### Configuration
- Split DI registration into modular extension methods:
  - `AddAuthServices()` - Authentication and JWT configuration
  - `AddPersistenceServices()` - DbContext, Identity, and repositories
  - `UseCrossCuttingConcerns()` - Middleware registration
- Updated MediatR assembly scanning for new feature structure
- Updated FluentValidation assembly scanning for new feature structure

#### Swagger Configuration
- Fixed OpenAPI version compatibility issue
- Downgraded from Swashbuckle.AspNetCore v10.x to v9.0.6
- Restored standard dictionary initialization for JWT Bearer security definition
- Maintained JWT Bearer authentication support in Swagger UI

### 🗑️ Removed

#### Deleted Projects
- **PizzaStore.Infrastructure** - Split into focused Core projects
- **PizzaStore.Tests** - Replaced with 6 individual test projects

#### Deleted Directories
- `src/PizzaStore.Application/Commands/` - Moved to feature-based structure
- `src/PizzaStore.Application/Queries/` - Moved to feature-based structure
- `src/PizzaStore.Application/DTOs/` - Moved to Core.Auth.DTOs
- `src/PizzaStore.Application/Validators/` - Moved to feature folders
- `src/PizzaStore.Application/Interfaces/` - Moved to Core.Auth
- `src/PizzaStore.Application/Common/Exceptions/` - Moved to Core.CrossCuttingConcerns
- `src/PizzaStore.API/Middleware/` - Moved to Core.CrossCuttingConcerns

#### Deleted Files (31 total)
- All files from old Infrastructure project
- Old type-based organized files in Application
- Old monolithic test project files
- Inline seed data logic from Program.cs

### 🔧 Fixed

- Resolved circular dependency between Application and Core.Auth
- Fixed Swagger OpenAPI compatibility issues with proper versioning
- Corrected assembly scanning paths for MediatR and FluentValidation
- Fixed namespace inconsistencies across all projects

### 📦 Dependencies

#### Added
- **Core.Auth:**
  - Microsoft.AspNetCore.Identity
  - Microsoft.AspNetCore.Authentication.JwtBearer
  - System.IdentityModel.Tokens.Jwt
  
- **Core.CrossCuttingConcerns:**
  - Microsoft.AspNetCore.Http.Abstractions
  - Microsoft.Extensions.Logging.Abstractions
  
- **Infrastructure.Persistence:**
  - Microsoft.AspNetCore.Identity.EntityFrameworkCore
  - Microsoft.EntityFrameworkCore
  - Microsoft.EntityFrameworkCore.InMemory
  - Microsoft.Extensions.Identity.Core
  
- **All Test Projects:**
  - xUnit
  - xUnit.runner.visualstudio
  - coverlet.collector

#### Changed
- **API Project:**
  - Swashbuckle.AspNetCore: Downgraded from v10.x to v9.0.6
  - Added references to Core.Auth, Core.CrossCuttingConcerns, Infrastructure.Persistence

### 🏗️ Technical Improvements

#### SOLID Principles Implementation
- **Single Responsibility:** Each project and class has one clear responsibility
- **Open/Closed:** Extension methods allow adding new features without modifying existing code
- **Liskov Substitution:** Repository pattern and interfaces follow LSP
- **Interface Segregation:** Small, focused interfaces (IAuthService, IJwtTokenGenerator)
- **Dependency Inversion:** All dependencies flow toward abstractions

#### DRY Principles
- Eliminated code duplication with shared Core modules
- Centralized exception handling in one middleware
- Reusable Auth services across features
- Common repository base class

#### Naming Conventions
- **Projects:** `PizzaStore.{Layer}.{Module}` pattern
- **Features:** `{Action}{Entity}{Type}` pattern
- **Namespaces:** Match physical folder structure
- **Extensions:** `{Module}ServiceExtensions` pattern

### 📊 Statistics

- **Files Changed:** 31 files modified/moved/deleted
- **Lines Changed:** +175 additions, -804 deletions (net -629 lines)
- **Projects Added:** 9 new projects (3 Core + 6 Test)
- **Projects Removed:** 2 old projects
- **Total Projects:** 12 (up from 5)

### 🧪 Testing Infrastructure

- Created 6 individual test projects for isolated testing
- Test projects mirror source project structure
- Ready for unit, integration, and end-to-end tests
- xUnit framework configured with code coverage support

### 📝 Documentation

- Updated README.md with new architecture diagrams
- Added feature-based organization guidelines
- Documented SOLID and DRY principles implementation
- Added naming conventions documentation
- Created this comprehensive CHANGELOG.md

### ⚠️ Breaking Changes

This is a major version bump due to significant architectural changes:

1. **Namespace Changes:**
   - All Application namespaces changed from type-based to feature-based
   - Example: `PizzaStore.Application.Commands.Auth` → `PizzaStore.Application.Features.Commands.Auth.Register`

2. **Project References:**
   - `PizzaStore.Infrastructure` no longer exists
   - Must reference `Core.Auth`, `Core.CrossCuttingConcerns`, and `Infrastructure.Persistence` separately

3. **DTO Locations:**
   - Auth DTOs moved from `Application.DTOs` to `Core.Auth.DTOs`
   - Update using statements accordingly

4. **DI Registration:**
   - Old: `services.AddInfrastructure(configuration)`
   - New: `services.AddAuthServices(configuration)`, `services.AddPersistenceServices(configuration)`

### 🚀 Migration Guide

If upgrading from v1.x to v2.0:

1. **Update Project References:**
   ```xml
   <!-- Remove -->
   <ProjectReference Include="..\PizzaStore.Infrastructure\PizzaStore.Infrastructure.csproj" />
   
   <!-- Add -->
   <ProjectReference Include="..\PizzaStore.Core.Auth\PizzaStore.Core.Auth.csproj" />
   <ProjectReference Include="..\PizzaStore.Core.CrossCuttingConcerns\PizzaStore.Core.CrossCuttingConcerns.csproj" />
   <ProjectReference Include="..\PizzaStore.Infrastructure.Persistence\PizzaStore.Infrastructure.Persistence.csproj" />
   ```

2. **Update Using Statements:**
   ```csharp
   // Old
   using PizzaStore.Application.Commands.Auth;
   using PizzaStore.Application.DTOs;
   
   // New
   using PizzaStore.Application.Features.Commands.Auth.Register;
   using PizzaStore.Application.Features.Commands.Auth.Login;
   using PizzaStore.Core.Auth.DTOs;
   ```

3. **Update DI Registration in Program.cs:**
   ```csharp
   // Old
   builder.Services.AddInfrastructure(builder.Configuration);
   
   // New
   builder.Services.AddAuthServices(builder.Configuration);
   builder.Services.AddPersistenceServices(builder.Configuration);
   app.UseCrossCuttingConcerns();
   ```

4. **Update MediatR/FluentValidation Scanning:**
   ```csharp
   // Old
   builder.Services.AddMediatR(typeof(RegisterUserCommand).Assembly);
   
   // New
   builder.Services.AddMediatR(cfg => 
       cfg.RegisterServicesFromAssembly(
           typeof(PizzaStore.Application.Features.Commands.Auth.Register.RegisterUserCommand).Assembly
       ));
   ```

### ✅ Verification

The refactored application has been verified to:
- Build successfully with 0 errors (all 12 projects)
- Run successfully on http://localhost:5282
- Maintain all existing functionality (auth endpoints, Swagger UI)
- Follow Clean Architecture dependency rules
- Implement SOLID and DRY principles throughout

---

## [1.0.0] - 2026-01-30

### Initial Release

- Basic Clean Architecture structure with 4 projects
- ASP.NET Core Identity authentication
- JWT Bearer token support
- MediatR CQRS implementation
- Repository and Unit of Work patterns
- Global exception handling
- FluentValidation
- Swagger/OpenAPI documentation
- In-memory database for development
- User registration and login endpoints

---

**Legend:**
- ✨ Added: New features
- 🔄 Changed: Changes in existing functionality
- 🗑️ Removed: Removed features
- 🔧 Fixed: Bug fixes
- 📦 Dependencies: Dependency changes
- ⚠️ Breaking Changes: Breaking changes requiring migration
