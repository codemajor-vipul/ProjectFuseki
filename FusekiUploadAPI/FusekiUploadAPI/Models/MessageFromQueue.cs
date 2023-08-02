using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FusekiUploadAPI.Models
{
    public class MessageFromQueue
    {
        public string Message { get; set; }
        public string Id { get; set; }
        public string DataSetName { get; set; }
    }
}
