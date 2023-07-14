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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DataContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("developmentConnection")));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddScoped<ICharacterService, CharacterService>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateCharacterReqValidator>();
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

app.UseAuthorization();

app.MapControllers();

app.Run();
