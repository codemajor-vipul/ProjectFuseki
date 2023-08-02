using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FusekiUploadAPI.Fuseki
{
    public interface IFusekiActivity
    {
        public bool InsertToGraph(string Query,string DataSet);
    }
}
