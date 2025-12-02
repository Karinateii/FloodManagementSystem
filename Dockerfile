# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["FloodManagementSystem/GlobalDisasterManagement.csproj", "FloodManagementSystem/"]
RUN dotnet restore "FloodManagementSystem/GlobalDisasterManagement.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/FloodManagementSystem"
RUN dotnet build "GlobalDisasterManagement.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "GlobalDisasterManagement.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Create uploads directory
RUN mkdir -p /app/wwwroot/Uploads

# Copy published app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "GlobalDisasterManagement.dll"]
