using System.Collections.Concurrent;
using System.Reflection;
using ChatApplication.Business.Abstract;
using ChatApplication.Business.BackgroundJobs;
using ChatApplication.Business.Concrete;
using ChatApplication.DataAccess.Abstract;
using ChatApplication.DataAccess.Concrete;
using ChatApplication.DataAccess.Entities;
using ChatApplication.DataAccess.Entities.Enums;
using ChatApplication.Middlewares;
using Newtonsoft.Json;
using ChatSession = ChatApplication.DataAccess.Entities.ChatSession;
using Team = ChatApplication.DataAccess.Entities.Team;

namespace ChatApplication;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

        builder.Services.AddControllers()
            .AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);
        
        builder.Services.AddSingleton<IChatQueueManager, ChatQueueManager>();
        builder.Services.AddScoped<IChatQueueService, ChatQueueService>();
        builder.Services.AddSingleton<ITeamService>(_ => new TeamService(SetInitialTeams().Teams, SetInitialTeams().OverflowTeam));
        builder.Services.AddScoped<IChatSessionService, ChatSessionService>();
        
        builder.Services.AddSingleton(new ConcurrentDictionary<Guid, ChatSession>());
        
        builder.Services.AddHttpClient();
        builder.Services.AddHostedService<PollSimulationJob>();
        builder.Services.AddHostedService<AgentAssignmentJob>();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<RequestTrackingMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        
        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }

    private static (List<Team> Teams, DataAccess.Entities.OverflowTeam OverflowTeam) SetInitialTeams()
    {
        var teams = new List<Team>
        {
            new ()
            {
                Id = 1,
                Name = "Team A",
                Shift = Shift.Morning,
                Agents = new List<Agent>
                {
                    new () { Name = "Team Lead A", AgentType = AgentType.TeamLead },
                    new () { Name = "Mid-Level A1", AgentType = AgentType.MidLevel },
                    new () { Name = "Mid-Level A2", AgentType = AgentType.MidLevel },
                    new () { Name = "Junior A", AgentType = AgentType.Junior }
                }
            },
            new ()
            {
                Id = 2,
                Name = "Team B",
                Shift = Shift.Afternoon,
                Agents = new List<Agent>
                {
                    new () { Name = "Senior B", AgentType = AgentType.Senior },
                    new () { Name = "Mid-Level B", AgentType = AgentType.MidLevel },
                    new () { Name = "Junior B1", AgentType = AgentType.Junior },
                    new () { Name = "Junior B2", AgentType = AgentType.Junior }
                }
            },
            new ()
            {
                Id = 3,
                Name = "Team C",
                Shift = Shift.Night,
                Agents = new List<Agent>
                {
                    new () { Name = "Mid-Level C1", AgentType = AgentType.MidLevel },
                    new () { Name = "Mid-Level C2", AgentType = AgentType.MidLevel }
                }
            }
        };

        var overflowTeam = new OverflowTeam
        {
            Id = 4,
            Name = "Team Overflow",
            Agents = new List<Agent>
            {
                new () { Name = "Overflow1", AgentType = AgentType.Overflow },
                new () { Name = "Overflow2", AgentType = AgentType.Overflow },
                new () { Name = "Overflow3", AgentType = AgentType.Overflow },
                new () { Name = "Overflow4", AgentType = AgentType.Overflow },
                new () { Name = "Overflow5", AgentType = AgentType.Overflow },
                new () { Name = "Overflow6", AgentType = AgentType.Overflow }
            }
        };

        return (teams, overflowTeam);
    }
}