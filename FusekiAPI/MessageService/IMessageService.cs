using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageService
{
    public interface IMessageService
    {
        public void PublishMessage(IModel channel,string message);
    }
}
