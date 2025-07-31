using DemoWindowService;

var builder = Host.CreateApplicationBuilder(args);

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHttpClient(); // Register IHttpClientFactory
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
