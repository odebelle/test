using BusRepositoryApi;
using BusRepositoryApi.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var conf = builder.Configuration;
var srv = builder.Services;
// Add services to the container.
var cn = conf.GetConnectionString(nameof(BusRemoteOperatorContext))!;

srv.AddDbContext<BusRemoteOperatorContext>(x =>
        x.UseNpgsql(cn).UseSnakeCaseNamingConvention(),
    ServiceLifetime.Transient);

builder.Services.AddTransient<IPersistenceRepository, PersistenceRepository>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
srv.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddKeycloakSettings();
builder.AddKeycloakAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
//app.UseKeycloakAuthentication();
app.UseAuthorization();
app.MapControllers().RequireAuthorization(Policies.DispatcherOperator);

app.Run();