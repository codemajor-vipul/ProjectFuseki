using FusekiAPI.SagaPattern.Saga;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FusekiAPI.SagaPattern.Infrastructure
{
    public interface IInsertIntoGraph
    {
        public Task<string> InsertIntoGraph(InsertIntoGraphSaga saga);
    }
}
