# Build the application
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /source

# Copy the necessary source files from your solution
COPY . .

# Restore the dependencies for the API project
RUN dotnet restore "RPSLSGameServiceAPI/RPSLSGameServiceAPI.csproj"

# Publish the application to the /app/publish directory
RUN dotnet publish "RPSLSGameServiceAPI/RPSLSGameServiceAPI.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
WORKDIR /app

# Create a directory for the SQLite database
RUN mkdir -p /app/db

# Copy the app to the container
COPY --from=build /app/publish .

# Copy the SQLite file to the /app/db directory
COPY RPSLSGameServiceAPI/RPSLSGame.db /app/db

# Set the environment variable for ASP.NET Core environment
ENV ASPNETCORE_ENVIRONMENT=Docker

# Expose the port your application listens on
EXPOSE 80

# Set the entry point
ENTRYPOINT ["dotnet", "RPSLSGameServiceAPI.dll"]
