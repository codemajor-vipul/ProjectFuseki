using FusekiAPI.Parser;
using FusekiAPI.RabbitConnection;
using FusekiAPI.SagaPattern.Infrastructure;
using FusekiAPI.SagaPattern.Orchestrator;
using FusekiAPI.SagaPattern.Saga;
using FusekiAPI.SagaPattern.Steps;
using MessageService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Interface;

namespace FusekiAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FusekiController : ControllerBase
    {
        private IConfiguration _iconfiguration;
        private IMessageService _messageService;
        private IWebHostEnvironment _webhost;
        private IWorkflowHost _workFlowHost;
        private IParser _parser;
        private IInsertIntoGraph _insertIntoGraph;
        public FusekiController(IConfiguration configuration,IWebHostEnvironment webHost,IMessageService messageService,IParser parser,IWorkflowHost workflowHost,IInsertIntoGraph insertIntoGraph)
        {
            _iconfiguration = configuration;
            _messageService = messageService;
            _webhost = webHost;
            _parser = parser;
            _workFlowHost = workflowHost;
            _insertIntoGraph = insertIntoGraph;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("The Api is running successfully");
        }

        [Authorize("ClientIdPolicy")]
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var message = Request.Form["GraphUri"].ToString();
            var files = Request.Form.Files;
            using (var inputStream = new FileStream(_webhost.ContentRootPath+"\\RdfFiles\\"+files[0].FileName, FileMode.Create))
            {
                files[0].CopyTo(inputStream);
            }
            RabbitConnectionFactory rabbitConnectionFactory = new RabbitConnectionFactory(_iconfiguration);
            message = _parser.RdfFileParser(_webhost.ContentRootPath + "\\RdfFiles\\" + files[0].FileName, Request.Form["GraphUri"].ToString());
            try
            {
                InsertIntoGraphSaga saga = new InsertIntoGraphSaga()
                {
                    Message = message,
                    Id = Guid.NewGuid().ToString(),
                    DataSetName = Request.Form["DataSetName"].ToString()
                };
                var res = await _insertIntoGraph.InsertIntoGraph(saga);
                if(res == "Fail")
                {
                    return StatusCode(StatusCodes.Status400BadRequest,res);
                }
            }
            catch(Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
            return Ok("Data pushed to queue successfully !");
        }
    }
}
