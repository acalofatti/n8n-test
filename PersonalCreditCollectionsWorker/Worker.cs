using PersonalCreditCollectionsWorker.Contracts;

namespace PersonalCreditCollectionsWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly INovedadesProcessor _processor;

        public Worker(ILogger<Worker> logger, INovedadesProcessor processor)
        {
            _logger = logger;
            _processor = processor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker ejecutando a las {time}", DateTimeOffset.Now);

                try
                {
                    await _processor.ProcesarNovedadesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando novedades");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

}
