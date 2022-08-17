using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace micro_service_shared
{
    public class RabbitMqClient : IBusClient
    {
        private readonly ILogger<RabbitMqClient> _logger;
        private readonly IConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMqClient(ILogger<RabbitMqClient> logger, IConnectionFactory connectionFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public T? GetMessage<T>(string queueName, CancellationToken stoppingToken) where T: class
        {
            var result = _channel.BasicGet(queueName, false);

            if (result is not null)
            {
                var body = Encoding.UTF8.GetString(result.Body.ToArray());
                var message = JsonSerializer.Deserialize<T>(body);

                _channel.BasicAck(result.DeliveryTag, false);

                return message;
            }

            return null;
        }

        public bool Publish<T>(T model, CancellationToken stoppingToken)
        {
            if (IsReady() is not true)
            {
                if (ConnectToRabbitMq(default) is false)
                {
                    return false;
                }
            }

            var payload = JsonSerializer.Serialize(model);
            return PublishMessage(typeof(T).Name, payload);
        }

        public bool Subscribe<T>(string queueName, MyFunction<T> func, CancellationToken stoppingToken) where T: class
        {
            if (IsReady() is not true)
            {
                if (ConnectToRabbitMq(default) is false)
                {
                    return false;
                }
            }

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (object sender, BasicDeliverEventArgs @event) =>
            {
                var body = Encoding.UTF8.GetString(@event.Body.ToArray());
                var message = JsonSerializer.Deserialize<T>(body);
                _logger.LogInformation($"Received a new command: {message}");

                func(message);

                _channel.BasicAck(@event.DeliveryTag, false);
                _logger.LogInformation($"Sent Ack on Delivery Tag {@event.DeliveryTag}");

                await Task.Yield();
            };

            try
            {
                _channel.QueueDeclare(queue: queueName, autoDelete: false, exclusive: false, durable: true);

                _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            }
            catch (Exception ex)
            {
                _logger.LogError(default, ex, ex.Message);
                return false;
            }

            return true;
        }

        public void CloseConnection()
        {
            _channel?.Close();
            _channel?.Dispose();

            _connection?.Close();
            _connection?.Dispose();
        }

        private bool PublishMessage(string queueName, string payload)
        {
            try
            {
                _channel.QueueDeclare(queue: queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

                var props = _channel.CreateBasicProperties();
                props.AppId = "WebApi";
                props.Persistent = true;
                props.UserId = "guest";
                props.MessageId = Guid.NewGuid().ToString("N");
                props.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                var body = Encoding.UTF8.GetBytes(payload);
                _channel.BasicPublish("", queueName, props, body);

                _logger.LogInformation($"Sent publish message {props.MessageId} to {queueName}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(default, ex, ex.Message);
                return false;
            }
        }

        private bool ConnectToRabbitMq(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested is not true)
            {
                try
                {
                    if (ConnectionReady() is false)
                    {
                        _connection = _connectionFactory.CreateConnection();
                        _logger.LogInformation("Created connection to RabbitMq");
                    }

                    if (ModelReady() is false)
                    {
                        _channel = _connection.CreateModel();
                        _logger.LogInformation("Created model of RabbitMq");
                    }

                    return true;
                }
                catch (BrokerUnreachableException ex)
                {
                    _logger.LogError($"{ex.Message} {(_connectionFactory as ConnectionFactory)?.HostName}:{(_connectionFactory as ConnectionFactory)?.Port}");
                    Thread.Sleep(5000);
                }
                catch (Exception ex)
                {
                    _logger.LogError(default, ex, ex.Message);
                    return false;
                }
            }

            return false;
        }

        private bool ConnectionReady() => _connection is not null && _connection.IsOpen is true;
        private bool ModelReady() => _channel is not null && _channel.IsOpen is true;
        private bool IsReady() => ConnectionReady() && ModelReady();
    }
}

