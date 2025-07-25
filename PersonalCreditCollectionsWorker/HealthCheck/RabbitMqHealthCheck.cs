using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace PersonalCreditCollectionsWorker.HealthCheck
{
    public class RabbitMqHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _config;

        public RabbitMqHealthCheck(IConfiguration config)
        {
            _config = config;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _config["HostName"],
                    Port = int.Parse(_config["AMQPPort"]!),
                    UserName = _config["UserName"],
                    Password = _config["Password"],
                    VirtualHost = _config["VirtualHost"]
                };

                using var conn = factory.CreateConnection();
                return Task.FromResult(HealthCheckResult.Healthy("RabbitMQ is reachable"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("RabbitMQ unreachable", ex));
            }
        }
    }
}
