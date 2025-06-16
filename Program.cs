global using RPG_dotnet.Models;
global using RPG_dotnet.Services.CharactersService;
global using RPG_dotnet.Services.LoadoutService;
global using RPG_dotnet.Services.GameSessionService;
global using RPG_dotnet.Dtos.Characters;
global using RPG_dotnet.Dtos.Loadout;
global using AutoMapper;
global using RPG_dotnet.Middleware;
global using System.Net;
global using Newtonsoft.Json;
global using Microsoft.Data.SqlClient;
global using Serilog;
global using Serilog.Formatting.Elasticsearch;
global using Microsoft.EntityFrameworkCore;
global using RPG_dotnet.Data;
global using Microsoft.AspNetCore.Mvc;
global using FluentValidation;
global using RPG_dotnet.Validations;
global using FluentValidation.Results;
global using Microsoft.AspNetCore.Mvc.ModelBinding;
global using FluentValidation.AspNetCore;
global using RPG_dotnet.Helpers;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;
global using RPG_dotnet.Dtos.User;
global using System.Security.Claims;
global using Microsoft.IdentityModel.Tokens;
global using System.IdentityModel.Tokens.Jwt;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Swashbuckle.AspNetCore.Filters;
global using Microsoft.AspNetCore.Mvc.Filters;
global using RPG_dotnet.Controllers;
global using RPG_dotnet.Dtos.GameSession;
using Microsoft.OpenApi.Models;
using dotenv.net;
using Elastic.Apm.NetCoreAll;
using RPG_dotnet.Filters;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.File;

var builder = WebApplication.CreateBuilder(args);
DotEnv.Load();
string connString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
string token = Environment.GetEnvironmentVariable("TOKEN");
string esUri = Environment.GetEnvironmentVariable("ELASTIC_URI");
string esUsername = Environment.GetEnvironmentVariable("ELASTIC_USERNAME");
string esPassword = Environment.GetEnvironmentVariable("ELASTIC_PASSWORD");
if (connString is null || token is null || esUri is null || esUsername is null || esPassword is null)
    throw new NotFoundException("Missing environment variables");
// Add services to the container.
builder.Services.AddDbContext<DataContext>(options =>
options.UseSqlServer(connString));
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ServiceResponseLogFilter>();
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = """Standard Authorization header using the Bearer scheme. Example: "bearer {token}" """,
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.OperationFilter<SecurityRequirementsOperationFilter>();
});
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddScoped<ICharacterService, CharacterService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILoadoutService, LoadoutService>();
builder.Services.AddScoped<IGameSessionService, GameSessionService>();
builder.Services.AddScoped<AllowUnauthenticatedAttribute>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateCharacterReqValidator>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8
                    .GetBytes(token)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                await Task.Run(() => throw new AuthException());
            }
        };
    });
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .MinimumLevel.Information()
    // --- Start Overrides to reduce clutter from Microsoft and EF Core ---

    // Suppress Information/Debug logs from most Microsoft components by default
    // Only Warning, Error, and Fatal logs will pass through.
    // will comment this out in the future and target the sources one by one, right now clutter has been reduced enough
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)

    // This will suppress Info/Debug from core ASP.NET components
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    // Hide EF Core queries
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)

    // Specific override for the "Executed action..."
    // This targets the ControllerActionInvoker specifically
    .MinimumLevel.Override("Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker", LogEventLevel.Warning)

    // Other common noisy sources
    // .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Warning) // For "Request starting/finished"
    // .MinimumLevel.Override("Microsoft.AspNetCore.Routing.EndpointMiddleware", LogEventLevel.Warning) // For "Executing endpoint"


    .WriteTo.Console(new ElasticsearchJsonFormatter())
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(esUri))
    {
        AutoRegisterTemplate = false,
        IndexFormat = "dotnet-app-logs-{0:yyyy.MM}",
        ModifyConnectionSettings = x => x
            .ApiKeyAuthentication(esUsername, esPassword)
            .ServerCertificateValidationCallback((sender, cert, chain, sslPolicyErrors) => true)
            .RequestTimeout(TimeSpan.FromSeconds(60)),
        EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                           EmitEventFailureHandling.WriteToFailureSink |
                           EmitEventFailureHandling.ThrowException
    })
    .CreateLogger();

builder.Services.AddSingleton(Log.Logger);

builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCorrelationId();
app.UseMiddleware<ExceptionHandlingMiddleware>();
//not needed for now app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
