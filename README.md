# Product Management API

RESTful backend API built using ASP.NET Core and .NET 8.

## Technology Stack

- .NET 8
- ASP.NET Core Web API
- C#
- SQL Server
- Entity Framework Core
- JWT Authentication
- Refresh Token Rotation
- Role-Based Authorization
- FluentValidation
- AutoMapper
- API Versioning
- Swagger / OpenAPI
- Serilog
- xUnit
- Moq
- WebApplicationFactory

## Architecture

The application follows a layered architecture:

- API
- Application
- Domain
- Infrastructure

## Features

- Product CRUD operations
- Item management through product relationships
- JWT authentication
- Refresh token strategy
- Role-based authorization
- Global exception handling
- FluentValidation
- API versioning
- Structured logging
- Swagger documentation
- Unit testing
- Integration testing

## Project Structure

```text
ProductManagement
│
├── src
│   ├── ProductManagement.API
│   ├── ProductManagement.Application
│   ├── ProductManagement.Domain
│   └── ProductManagement.Infrastructure
│
└── tests
    ├── ProductManagement.API.Tests
    ├── ProductManagement.Application.Tests
    └── ProductManagement.Infrastructure.Tests
