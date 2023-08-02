using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FusekiAPI.Parser
{
    public interface IParser
    {
        public string RdfFileParser(string FilePath,string GraphUri);
    }
}
