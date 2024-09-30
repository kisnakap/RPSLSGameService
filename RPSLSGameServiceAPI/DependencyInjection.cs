using Microsoft.Extensions.DependencyInjection;
using RPSLSGameService.Application.Handlers;
using RPSLSGameService.Application.RPSLSCommands.Requests;
using RPSLSGameService.Application.RPSLSQueries.Interfaces;
using RPSLSGameService.Application.RPSLSQueries.Requests;
using RPSLSGameService.Infrastructure.Interfaces;
using RPSLSGameService.Infrastructure.Repositories;
using RPSLSGameService.Services;
using RPSLSGameService.Utilities;
using RPSLSGameService.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace RPSLSGameService.Infrastructure
{
    public static class DependencyInjection
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            // Command Handlers
            services.AddTransient<ICommandHandler<PlayRoundCommand>, PlayRoundHandler>();
            services.AddTransient<ICommandHandler<PlayMultiplayerCommand>, MultiplayerRoundHandler>();
            services.AddTransient<ICommandHandler<CreateSessionCommand>, CreateSessionHandler>();
            services.AddTransient<ICommandHandler<ResetScoreboardCommand>, ResetScoreboardHandler>();

            // Query Handlers
            services.AddTransient<IQueryHandler<GetScoreboardQuery, IActionResult>, GetScoreboardHandler>();
            services.AddTransient<IQueryHandler<GetRandomChoiceQuery, IActionResult>, GetRandomChoiceHandler>();

            // Repositories
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IGameSessionRepository, GameSessionRepository>();
            services.AddScoped<IMatchResultRepository, MatchResultRepository>();
            services.AddScoped<IPlayerRepository, PlayerRepository>();

            // Services
            services.AddSingleton<RandomChoiceService>();

            // Validators
            services.AddTransient<IValidator<RPSLSEnum>, ChoiceValidator>();
        }
    }
}
