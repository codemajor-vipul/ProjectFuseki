using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace FusekiAPI.Parser
{
    public class Parser : IParser
    {
        IGraph sampleGraph = new Graph();
        public string RdfFileParser(string FilePath,string GraphUri)
        {
            FileLoader.Load(sampleGraph, FilePath);
            StringBuilder builder = new StringBuilder();
            builder.Append("Insert Data{\n Graph <http://sampledata.com/>{\n");
            foreach (Triple triple1 in sampleGraph.Triples)
            {
                //Console.WriteLine(triple1.Subject + " " + triple1.Predicate + " " + triple1.Object);
                builder.Append("<" + triple1.Subject + "> " + "<" + triple1.Predicate + "> " + "\"" + triple1.Object + "\".\n");
            }
            builder.Append("}\n}");
            return builder.ToString();
        }
    }
}
