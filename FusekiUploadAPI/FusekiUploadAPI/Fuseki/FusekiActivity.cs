using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FusekiUploadAPI.Fuseki
{
    public class FusekiActivity : IFusekiActivity
    {
        public bool InsertToGraph(string Query,string DataSet)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:3030/"+DataSet);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/sparql-update"));
            FormUrlEncodedContent content = new FormUrlEncodedContent(
              new[] { new KeyValuePair<string, string>("update", Query) }
            );
            var response = httpClient.PostAsync(new Uri("http://localhost:3030/"+DataSet), content).Result;
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }
    }
}
