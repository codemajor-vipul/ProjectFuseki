using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FusekiAPI.SagaPattern.Saga
{
    public class InsertIntoGraphSaga
    {
        public string Message { get; set; }
        public string Id { get; set; }
        public string DataSetName { get; set; }
    }
}
