RPSLS Game Service

Overview
This project implements a REST API for playing the game Rock, Paper, Scissors, Lizard, Spock using .NET 5.0. It includes endpoints for playing the game (against computer or other player), retrieving choices, and maintaining a scoreboard. The project is containerized using Docker.

Requirements
Before you begin, ensure you have the following installed:
.NET 5.0 SDK
Docker
Visual Studio or any code editor of your choice.

Setup Instructions

Clone the Repository
Open a terminal and run the following command to clone the repository:
git clone <repository_url>
cd <repository_directory>

Build and Run Locally without Docker
If you want to run the project locally without Docker, follow these steps:
Open the project in your IDE (like Visual Studio or Visual Studio Code).

Restore dependencies using the .NET CLI:
dotnet restore

Database setup:
dotnet ef migrations add InitialCreate --project "<repository_directory>\RPSLSGameService\RPSLSGameService.Infrastructure\RPSLSGameService.Infrastructure.csproj" --startup-project "<repository_directory>\RPSLSGameService\RPSLSGameServiceAPI\RPSLSGameServiceAPI.csproj"

dotnet ef database update --project "<repository_directory>\RPSLSGameService
\RPSLSGameService.Infrastructure\RPSLSGameService.Infrastructure.csproj" --startup-project "<repository_directory>\RPSLSGameService\RPSLSGameServiceAPI\RPSLSGameServiceAPI.csproj"

Run the application:
dotnet run
The API should now be running at http://localhost:44308/ or http://localhost:44308/index.html (or where specified in your launchSettings.json).

Build and Run with Docker
If you prefer using Docker to run the application:
Navigate to the project root directory (where the Dockerfile is located).

Build the Docker image:
docker build -t rpslsgameserviceapi .

Run the Docker container:
docker run -d -p 8080:80 --name rpslstestapi rpslsgameserviceapi
The API should now be accessible at http://localhost:8080/ or http://localhost:8080/index.html in your web browser.

Database Setup

The application uses SQLite for database management. Ensure that the RPSLSGame.db file is accessible in the specified path in your appsettings.json. If needed, you can adjust your connection string in the appsettings.json file.

Testing the API
You can test the endpoints using tools like:
Postman
Swagger UI (if using default configurations)

Here are the main endpoints:

Method	Endpoint	Description
GET	/choices	Get a list of all game choices
GET	/choice	Get a randomly generated choice
POST	/play	Play a round against the computer
POST	/createSession	Create a new game session
POST	/playMulti	Play a multiplayer round (2 players only)
GET	/scoreboard	Get the current scoreboard
POST	/resetScoreboard	Reset the scoreboard

Useful Information
Logging
Logging is configured using Serilog. Logs will be stored in the logs directory with a daily rolling interval.

Error Handling
An error handling middleware is included to manage and log errors gracefully.

Middleware
Custom middleware for logging and error handling has been implemented to enhance the observability of the application.

Dockerfile
The provided Dockerfile builds the project and sets up the environment necessary to run the API in a Docker container.