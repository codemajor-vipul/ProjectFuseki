using FusekiAPI.RabbitConnection;
using MessageService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace FusekiAPI.SagaPattern.Steps
{
    public class InsertIntoGraphStep : IStepBody
    {
        private IMessageService _messageService;
        private IConfiguration _iconfiguration;
        public InsertIntoGraphStep(IMessageService messageService,IConfiguration configuration)
        {
            _messageService = messageService;
            _iconfiguration = configuration;
        }
        public string Message { get; set; }
        public string Id { get; set; }
        public string DataSetName { get; set; }
        public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            RabbitConnectionFactory rabbitConnectionFactory = new RabbitConnectionFactory(_iconfiguration);
            _messageService.PublishMessage(rabbitConnectionFactory.ConnectToRabbitMQ(), JsonConvert.SerializeObject(new InsertIntoGraphStep(_messageService,_iconfiguration){ Message = Message,Id=Id,DataSetName = DataSetName }));
            return ExecutionResult.Next();
        }
    }
}
