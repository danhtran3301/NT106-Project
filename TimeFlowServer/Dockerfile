# Multi-stage build for optimal image size
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["TimeFlowServer/TimeFlowServer.csproj", "TimeFlowServer/"]
RUN dotnet restore "TimeFlowServer/TimeFlowServer.csproj"

# Copy source code and build
COPY TimeFlowServer/ TimeFlowServer/
WORKDIR "/src/TimeFlowServer"
RUN dotnet build "TimeFlowServer.csproj" -c Release -o /app/build
RUN dotnet publish "TimeFlowServer.csproj" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install utilities for healthcheck and debugging
RUN apt-get update && apt-get install -y \
    curl \
    netcat-traditional \
    && rm -rf /var/lib/apt/lists/*

# Copy published files
COPY --from=build /app/publish .

# Create logs directory
RUN mkdir -p /app/logs && chmod 777 /app/logs

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Docker
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV TZ=Asia/Ho_Chi_Minh

# Expose port
EXPOSE 1010

# Health check using netcat to test TCP port
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD nc -z localhost 1010 || exit 1

# Entry point
ENTRYPOINT ["dotnet", "TimeFlowServer.dll"]
