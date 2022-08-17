using System;
namespace micro_service_shared
{
    public interface IBusClient
    {
        bool Publish<T>(T model, CancellationToken stoppingToken);
        T? GetMessage<T>(string queueName, CancellationToken stoppingToken) where T : class;
        bool Subscribe<T>(string queueName, MyFunction<T> func, CancellationToken stoppingToken) where T : class;
        void CloseConnection();
    }
}

