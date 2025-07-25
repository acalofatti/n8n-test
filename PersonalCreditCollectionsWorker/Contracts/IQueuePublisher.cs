using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalCreditCollectionsWorker.Contracts
{
    public interface IQueuePublisher
    {
        Task PublishAsync<T>(T message);
    }
}

