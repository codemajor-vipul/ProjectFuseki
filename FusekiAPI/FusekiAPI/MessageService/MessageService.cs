using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FusekiAPI.Models;
using FusekiAPI.RabbitConnection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WorkflowCore.Interface;

namespace FusekiAPI.MessageService
{
    public class MessageService : BackgroundService
    {
        private IConfiguration _iconfiguration;
        private IWorkflowHost _workflow;
       
        public MessageService(IConfiguration configuration,IWorkflowHost workFlow)
        {
            _iconfiguration = configuration;
            _workflow = workFlow;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RabbitConnectionFactory rabbitConnectionFactory = new RabbitConnectionFactory(_iconfiguration);
            IModel channel = rabbitConnectionFactory.ConnectToRabbitMQ();
            //channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = JsonConvert.DeserializeObject<ResponseMessage>(Encoding.UTF8.GetString(body));
                switch(message.Status)
                {
                    case "Success": _workflow.PublishEvent("SuccessfullTransaction", message.Id, message.Status);
                        break;
                    case "Fail": _workflow.PublishEvent("FailedTransaction", message.Id, message.Status);
                        break;
                }
            };
            channel.BasicConsume(queue: "FusekiTransactionsStatus",
                                 autoAck: true,
                                 consumer: consumer);
            return Task.Delay(5000, stoppingToken);
        }
    }
}
