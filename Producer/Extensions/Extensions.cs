using Dispatcher;
using Shared.Models;

namespace Producer.Extensions;

public static class Extensions
{
    public static IServiceCollection AddDispatcherContext<TContext>(this IServiceCollection services)
        where TContext : DispatcherContext
    {
        services.AddScoped<TContext>();
        return services;
    }

    public static IServiceCollection AddProducer<TContext>(this IServiceCollection services)
        where TContext : class, IDispatcherProducer
    {
        services.AddSingleton<IDispatcherProducer, TContext>();
        return services;
    }


    public static IConfigurationBuilder AddApiConfiguration(this IConfigurationBuilder builder)
    {
        //HttpClient apiClient = HttpClientFactory;

        return builder;
    }
}

public class ApiConfigurationContext(HttpMessageHandler? handler = null) : HttpClient(handler)
{
    public void SaveChanges(IDictionary<string, string?> settinqs)
    {
        //this.Send(request);
    }
}

public class ApiConfigurationProvider(string apiBaseUrl, string workerId) : ConfigurationProvider
{
    public override void Load()
    {
        ProducerElement pe = new ProducerElement();

        //Data = 
    }

    private static IDictionary<string, string?> CreateAndSaveDefaultValues(ApiConfigurationContext context)
    {
        var settings = new Dictionary<string, string?>(
            StringComparer.OrdinalIgnoreCase)
        {
            ["WidgetOptions:EndpointId"] = "b3da3c4c-9c4e-4411-bc4d-609e2dcc5c67",
            ["WidgetOptions:DisplayLabel"] = "Widgets Incorporated, LLC.",
            ["WidgetOptions:WidgetRoute"] = "api/widgets"
        };

        context.SaveChanges(settings);

        return settings;
    }
}