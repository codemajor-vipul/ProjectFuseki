using FusekiUploadAPI.Fuseki;
using FusekiUploadAPI.Models;
using FusekiUploadAPI.RabbitConnection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FusekiUploadAPI.MessageService
{
    public class MessageBroker : BackgroundService
    {
        private IConfiguration _iconfiguration;
        private IFusekiActivity _fusekiActivity;
        public MessageBroker(IConfiguration configuration,IFusekiActivity fusekiActivity)
        {
            _iconfiguration = configuration;
            _fusekiActivity = fusekiActivity;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RabbitConnectionFactory rabbitConnectionFactory = new RabbitConnectionFactory(_iconfiguration);
            IModel channel = rabbitConnectionFactory.ConnectToRabbitMQ();
            channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = JsonConvert.DeserializeObject<MessageFromQueue>(Encoding.UTF8.GetString(body));
                bool result = _fusekiActivity.InsertToGraph(message.Message, message.DataSetName);
                if(result)
                {
                    channel.BasicAck(ea.DeliveryTag, false);
                    channel.BasicPublish(
                            "ex.FusekiTransactions",
                            "",
                            null,
                            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new ResponseMessage(){ Id = message.Id, Status = "Success" })));
                }
                else
                {
                    channel.BasicReject(ea.DeliveryTag, false);
                    channel.BasicPublish(
                            "ex.FusekiTransactions",
                            "",
                            null,
                            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new ResponseMessage(){ Id = message.Id, Status = "Fail" })));
                }
            };
            channel.BasicConsume(queue: "FusekiQueue",
                                 autoAck: false,
                                 consumer: consumer);
            return Task.Delay(5000, stoppingToken);
        }
    }
}
