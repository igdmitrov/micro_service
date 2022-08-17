using micro_service_report_worker;
using micro_service_shared;
using RabbitMQ.Client;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration configuration = hostContext.Configuration;

        var appBusSettings = configuration
                                .GetSection("AppRabbitMqConnection")
                                .Get<AppRabbitMqSettings>();

        services.AddSingleton<IConnectionFactory, ConnectionFactory>(serviceProvider =>
        {
            return new ConnectionFactory
            {
                HostName = appBusSettings.Host,
                Port = appBusSettings.Port,
                UserName = appBusSettings.UserName,
                Password = appBusSettings.Password,
                DispatchConsumersAsync = true
            };
        });

        services.AddSingleton<IBusClient, RabbitMqClient>();

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();

