using Dispatcher;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Producer;
using Producer.Cron;
using Producer.Extensions;
using RabbitMq;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using Shared.Clients;
using Shared.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSystemd();

// Get configurations
var configuration = builder.Configuration;
var services = builder.Services;

var uris = configuration.GetValue<string>("Elastic:Uris")?.Split(',').Select(uri => new Uri(uri.Trim())) ??
           Enumerable.Empty<Uri>();
var usr = configuration.GetValue<string>("Elastic:usr");
var pwd = configuration.GetValue<string>("Elastic:pwd");

// Set ElasticSearch sink options
var sinkOptions = new ElasticsearchSinkOptions(uris)
{
    IndexFormat = $"sol-log-producer-{DateTime.Now:yyyy.MM.dd}",
    AutoRegisterTemplate = true,
    OverwriteTemplate = true,
    TemplateName = "SolarecLogProducer",
    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
    TypeName = null,
    BatchAction = ElasticOpType.Create,
    ModifyConnectionSettings = x => x.BasicAuthentication(usr, pwd)
};

/*builder.Host.UseSerilog((context, config) =>
{
    config
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .WriteTo.Console(LogEventLevel.Debug)
        .WriteTo.File(@"log.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3)
        .ReadFrom.Configuration(builder.Configuration)
        .WriteTo.Elasticsearch(sinkOptions);
});*/
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(LogEventLevel.Debug)
    //.WriteTo.File(@"log.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3)
    .WriteTo.Elasticsearch(sinkOptions)
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();


builder.Logging.AddSerilog();

// Add services to the container.
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);

services.Configure<HostOptions>(hostOptions =>
{
    hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.StopHost;
    hostOptions.ShutdownTimeout = new TimeSpan(0, 5, 0);
});

// Add Bro service

services.Configure<WorkerElements>(configuration.GetSection(nameof(WorkerElements)));
services.Configure<RemoteOperator>(configuration.GetSection(nameof(RemoteOperator)));
services.Configure<ElasticsearchSinkOptions>(configuration.GetSection(nameof(ElasticsearchSinkOptions)));

services.AddSingleton(sinkOptions);
services.AddSingleton<IProducerProcessManager, ProducerProcessManager>();
// services.AddDbContext<BusRemoteOperatorContext>(b=>b.UseSqlServer(busRemoteOperatorContext), ServiceLifetime.Transient);
services.AddTransient<IDeadLetterClient, DeadLetterClient>();

services.AddSingleton<IProducerElement, ProducerElement>();
services.AddTransient<IMqttWorkerService, MqttProducerService>();

services.AddHttpClient<IWorkerService, WorkerService>()
    .AddHttpMessageHandler<WorkerServiceHandler>();

services.AddProducer<RabbitProducer>();

services.AddHostedService<CronBackgroundService<DummyClass>>();

builder.Host.UseSerilog();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// app.UseAuthentication();
//
// app.UseRouting();
// app.UseAuthorization();

app.MapGet("/ping", () => "ping ok")
    .WithName("Ping")
    .WithOpenApi();

app.MapGet(pattern: "/status",
        ([FromBody] ProducerElement data, [FromServices] IDispatcherContext dispatcherContext) =>
        {
            dispatcherContext.PublishAsync(new[] { data });
        })
    .WithName("GetStatus")
    .WithOpenApi()
    .RequireAuthorization();

app.MapPost(pattern: "/status",
        ([FromBody] ProducerElement data, [FromServices] IDispatcherContext dispatcherContext) =>
        {
            dispatcherContext.PublishAsync(new ProducerElement[] { data });
        })
    .WithName("PostStatus")
    .WithOpenApi()
    .RequireAuthorization();

app.MapGet(pattern: "/pause",
        ([FromServices] IProcessManager processManager) => { processManager.Pause(); })
    .WithName("Pause")
    .WithOpenApi()
    .RequireAuthorization();

app.MapGet(pattern: "/resume",
        ([FromServices] IProcessManager processManager) => { processManager.Resume(); })
    .WithName("Resume")
    .WithOpenApi()
    .RequireAuthorization();

app.Run();