# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project files
COPY ["src/ProductManagement.API/ProductManagement.API.csproj", "src/ProductManagement.API/"]
COPY ["src/ProductManagement.Application/ProductManagement.Application.csproj", "src/ProductManagement.Application/"]
COPY ["src/ProductManagement.Domain/ProductManagement.Domain.csproj", "src/ProductManagement.Domain/"]
COPY ["src/ProductManagement.Infrastructure/ProductManagement.Infrastructure.csproj", "src/ProductManagement.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/ProductManagement.API/ProductManagement.API.csproj"

# Copy all source code
COPY . .

# Move to API project
WORKDIR "/src/src/ProductManagement.API"

# Publish
RUN dotnet publish "ProductManagement.API.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ProductManagement.API.dll"]