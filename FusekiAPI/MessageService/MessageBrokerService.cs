using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageService
{
    public class MessageBrokerService : IMessageService
    {
        public void PublishMessage(IModel channel,string message)
        {
            channel.BasicPublish(
                "ex.Fuseki",
                "fuseki123",
                null,
                Encoding.UTF8.GetBytes(message));
        }
    }
}
