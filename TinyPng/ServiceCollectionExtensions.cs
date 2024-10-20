using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TinyPng;

[PublicAPI]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTinyPngOptions(this IServiceCollection services, IConfiguration configuration)
    {
        return services.Configure<TinyPngOptions>(configuration.GetSection(TinyPngOptions.SectionName));
    }
    
    /// <summary>
    /// Adds the <see cref="TinyPngClient"/> to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="apiKey">The TinyPNG API key</param>
    /// <param name="httpClientBuilder">Optional customization of the <see cref="IHttpClientBuilder"/> in the <see cref="IHttpClientFactory"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    /// <remarks>This will use <see cref="IHttpClientFactory"/>, if it is registered. Otherwise, it will use a new HttpClient.</remarks>
    public static IServiceCollection AddTinyPng(this IServiceCollection services, Action<IHttpClientBuilder>? httpClientBuilder = null)
    {
        var clientName = AddHttpClient(services, httpClientBuilder);

        services.AddSingleton<ITinyPngHttpClient>(serviceProvider =>
        {
            var httpClient = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(clientName);
            return new TinyPngHttpClient(httpClient);
        });
        services.AddScoped<TinyPngClient>();
        return services;
    }

    /// <summary>
    /// Adds the <see cref="TinyPngClient"/> to the <see cref="IServiceCollection"/> as a singleton.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="httpClientBuilder">Optional customization of the <see cref="IHttpClientBuilder"/> in the <see cref="IHttpClientFactory"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    /// <remarks>This will use <see cref="IHttpClientFactory"/>, if it is registered. Otherwise, it will use a new HttpClient.</remarks>
    public static IServiceCollection AddTinyPngSingleton(this IServiceCollection services, Action<IHttpClientBuilder>? httpClientBuilder = null)
    {
        var clientName = AddHttpClient(services, httpClientBuilder);
        
        services.AddSingleton<ITinyPngHttpClient>(serviceProvider =>
        {
            var httpClient = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(clientName);
            return new TinyPngHttpClient(httpClient);
        });
        services.AddSingleton<TinyPngClient>();
        return services;
    }
    
    private static string AddHttpClient(IServiceCollection services, Action<IHttpClientBuilder>? httpClientBuilder)
    {
        var clientBuilder = services.AddHttpClient<TinyPngClient>((serviceProvider, configureClient) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<TinyPngOptions>>().Value;
            configureClient.BaseAddress = new Uri(options.ApiEndPoint);
            configureClient.DefaultRequestHeaders.Authorization = TinyPngClient.BuildAuthorizationHeader(options.ApiKey ?? throw new NullReferenceException($"{TinyPngOptions.SectionName}.{nameof(TinyPngOptions.ApiKey)} is required"));
        });
        clientBuilder.RedactLoggedHeaders(header => header.Equals("Authorization", StringComparison.InvariantCultureIgnoreCase));
        httpClientBuilder?.Invoke(clientBuilder);
        var clientName = clientBuilder.Name;
        return clientName;
    }
}