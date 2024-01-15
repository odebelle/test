using System.Net.Http.Headers;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using ServerSideOperator.Components;
using ServerSideOperator.Services;
using Shared.Clients;

var builder = WebApplication.CreateBuilder(args);
var conf = builder.Configuration;
var srv = builder.Services;

// Add services to the container.
var uris = conf.GetValue<string>("Elastic:Uris")!.Split(',').Select(uriString => new Uri(uriString));
var usr = builder.Configuration.GetValue<string>("Elastic:usr");
var pwd = builder.Configuration.GetValue<string>("Elastic:pwd");

var sinkOptions = new ElasticsearchSinkOptions(uris)
{
    IndexFormat = $"sol-log-bus-remote-operator-{DateTime.Now:yyyy.MM.dd}",
    AutoRegisterTemplate = true,
    OverwriteTemplate = true,
    TemplateName = "BusRemoteOperatorLog",
    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
    TypeName = null,
    MinimumLogEventLevel = LogEventLevel.Information,
    BatchAction = ElasticOpType.Create,
    ModifyConnectionSettings = x => x.BasicAuthentication(usr, pwd)
};

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(LogEventLevel.Debug)
    //.WriteTo.File(@"log.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3)
    .WriteTo.Elasticsearch(sinkOptions)
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Logging.AddSerilog();

/*var cn = conf.GetConnectionString(nameof(BusRemoteOperatorContext))!;
srv.AddDbContext<BusRemoteOperatorContext>(x => 
    x.UseNpgsql(cn, b=>b.MigrationsAssembly("ServerSideOperator")).
        UseSnakeCaseNamingConvention() , ServiceLifetime.Transient);

builder.Services.AddScoped<IPersistenceRepository, PersistenceRepository>();*/

builder.Services.AddHttpClient<WorkerPersistenceClient>();

builder.Services.AddSingleton<IMqttService, MqttService>();

builder.Services.Configure<ElasticsearchSinkOptions>(
    builder.Configuration.GetSection(nameof(ElasticsearchSinkOptions)));


builder.Services.AddCors(opt =>
{
    opt.AddPolicy(name: "DispatcherPolicy", policyBuilder =>
    {
        policyBuilder.WithOrigins("*.industrie.local");
        // policyBuilder.AllowAnyOrigin();
        policyBuilder.AllowAnyHeader();
        policyBuilder.AllowAnyMethod();
        policyBuilder.SetIsOriginAllowed(_ => true);
        policyBuilder.AllowCredentials();
    });
});
srv.AddControllers();

builder.Services.AddHttpClient<IWorkerClient, WorkerClient>(client =>
    {
        var hostString = builder.Configuration["Kestrel:Endpoints:Https:Url"];
        client.BaseAddress = new Uri(hostString);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    })
    //.AddHttpMessageHandler<ConfidentialHandler>()
    .AddPolicyHandler(GetRetryPolicy())
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));

builder.Services.AddHostedService<PingService>();

builder.Host.UseSerilog();

// builder.Services.AddAuthorization(options =>
// {
//     options.FallbackPolicy = options.DefaultPolicy;
//     options.AddPolicy("WorkerAdministrator",
//         policy => policy.RequireClaim("groups", "426571f2-b0a8-44a9-9899-ec589d631178"));
//     options.AddPolicy("Worker", policy => policy.RequireRole("Worker"));
// });

srv.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors("DispatcherPolicy");

app.UseStaticFiles();
app.UseAntiforgery();

// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
return;


static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}