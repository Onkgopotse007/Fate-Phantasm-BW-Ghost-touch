global using RPG_dotnet.Models;
global using RPG_dotnet.Services.CharactersService;
global using RPG_dotnet.Dtos.Characters;
global using AutoMapper;
global using RPG_dotnet.Middleware;
global using System.Net;
global using Newtonsoft.Json;
global using System.Data.SqlClient;
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
global using RPG_dotnet.Services.TeamService;
global using Microsoft.AspNetCore.Mvc.Filters;
global using RPG_dotnet.Controllers;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DataContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("developmentConnection")));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c=>{
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme{
        Description = """Standar Authorization header using the Bearer scheme. Example: "bearer {token}" """,
        In= ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.OperationFilter<SecurityRequirementsOperationFilter>();
});
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddScoped<ICharacterService, CharacterService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<SetUserIdFilterAttribute>();
builder.Services.AddScoped<AllowUnauthenticatedAttribute>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateCharacterReqValidator>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options=>
    {
        options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8
                    .GetBytes("top secret key placeholder")),
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
// TODO: implement logic to write error logs to file
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(new ElasticsearchJsonFormatter(), restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning)
    .CreateLogger();
builder.Logging.AddSerilog();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
