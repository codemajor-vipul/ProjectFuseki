using FusekiAPI.SagaPattern.Infrastructure;
using FusekiAPI.SagaPattern.Saga;
using FusekiAPI.SagaPattern.Steps;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Models.LifeCycleEvents;

namespace FusekiAPI.SagaPattern.Orchestrator
{
    public class InsertIntoGraphOrchestrator : IWorkflow<InsertIntoGraphSaga>,IInsertIntoGraph
    {
        private readonly IWorkflowHost _workFlowHost;

        private readonly IConfiguration _configuration;

        private readonly ConcurrentDictionary<string, TaskCompletionSource<LifeCycleEvent>> _completionSources;
        public InsertIntoGraphOrchestrator(IWorkflowHost workflowHost,IConfiguration configuration,ILifeCycleEventHub lifeCycleEventHub)
        {
            _workFlowHost = workflowHost;
            _configuration = configuration;
            lifeCycleEventHub.Subscribe(HandleWorkflowEvent);
            _completionSources = new ConcurrentDictionary<string, TaskCompletionSource<LifeCycleEvent>>();
        }

        public string Id => "InsertIntoGraphSaga";

        public int Version => 1;

        public void Build(IWorkflowBuilder<InsertIntoGraphSaga> builder)
        {
            builder.StartWith(context => Console.WriteLine("hello")).Saga(
                saga => saga.StartWith<InsertIntoGraphStep>()
                .Input(step => step.Message, data => data.Message)
                .Input(step=> step.Id, data => data.Id)
                .Input(step => step.DataSetName, data => data.DataSetName)
                .WaitFor("SuccessfullTransaction", data => data.Id)
                .OnError(WorkflowErrorHandling.Terminate)
                .Output(data => data.Message, step => step.EventData)
                .If(data => data.Message == "Success")
                .Do(then => Console.WriteLine("successfully ran my workflow !")));
        }
        public async Task<string> InsertIntoGraph(InsertIntoGraphSaga saga)
        {
            var instanceId = await _workFlowHost.StartWorkflow("InsertIntoGraphSaga", Version, saga);
            var inputIntoGraph = new TaskCompletionSource<LifeCycleEvent>();
            _completionSources.TryAdd(instanceId, inputIntoGraph);
            var inputIntoGraphTaskResponse = await inputIntoGraph.Task;
            var workflowInstance =
                await _workFlowHost.PersistenceStore.GetWorkflowInstance(inputIntoGraphTaskResponse.WorkflowInstanceId);
            var sagaData = workflowInstance.Data;
            var createSequenceResponse = (InsertIntoGraphSaga)sagaData;
            return createSequenceResponse.Message;
        }
        private void HandleWorkflowEvent(LifeCycleEvent workFlowEvent)
        {
            switch (workFlowEvent)
            {
                case WorkflowCompleted _:
                case WorkflowTerminated _:
                    if (_completionSources.TryGetValue(workFlowEvent.WorkflowInstanceId, out var createDocumentTask))
                    {
                        while (true)
                        {
                            var workFlowInstance = _workFlowHost.PersistenceStore
                                .GetWorkflowInstance(workFlowEvent.WorkflowInstanceId).Result;

                            var workFlowStatus = workFlowInstance.Status;

                            if (workFlowStatus == WorkflowStatus.Complete || workFlowStatus == WorkflowStatus.Terminated)
                            {
                                break;
                            }
                            Thread.Sleep(250);
                        }
                        createDocumentTask.TrySetResult(workFlowEvent);
                    }
                    break;
            }
        }
    }
}
