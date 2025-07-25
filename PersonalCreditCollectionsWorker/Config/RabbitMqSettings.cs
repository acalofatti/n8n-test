using System.ComponentModel.DataAnnotations;

namespace PersonalCreditCollectionsWorker.Config
{
    public class RabbitMqSettings
    {
        public string HostName { get; set; }
        public int AMQPPort { get; set; }
        public string VirtualHost { get; set; }
        public string QueueName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

}
