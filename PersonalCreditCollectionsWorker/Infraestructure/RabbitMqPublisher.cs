using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using PersonalCreditCollectionsWorker.Config;
using PersonalCreditCollectionsWorker.Contracts;
using System.Threading.Channels;

namespace PersonalCreditCollectionsWorker.Infraestructure
{
    public class RabbitMqPublisher : IQueuePublisher
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMqPublisher> _logger;
        private readonly RabbitMqSettings _settings;

        public RabbitMqPublisher(IConnectionFactory connectionFactory, IOptions<RabbitMqSettings> options, ILogger<RabbitMqPublisher> logger)
        {
            _logger = logger;
            _settings = options.Value;

            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare("cobranzasmdc.exchange", ExchangeType.Direct, durable: true);

            _channel.QueueDeclare(
                queue: _settings.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        public Task PublishAsync<T>(T message)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            _channel.BasicPublish(
                exchange: "cobranzasmdc.exchange",
                routingKey: "cobranzasmdc",
                basicProperties: null,
            body: body);

            return Task.CompletedTask;
        }
    }
}
