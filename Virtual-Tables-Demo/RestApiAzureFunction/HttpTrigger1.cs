using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using CoreCode;
using System.IO;
using Newtonsoft.Json;

namespace VirtualTablesDemo
{
    public static class HttpTrigger1
    {
        
        [Function("RateRequest")]
        public static HttpResponseData RateRequest([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req, FunctionContext context)
        {
            ILogger log = context.GetLogger("RateRequest");
            log.LogInformation("RateRequest call received!");

            
            HttpResponseData ToReturn = req.CreateResponse();
            ToReturn.StatusCode = HttpStatusCode.OK;
            ToReturn.WriteString(CoreCode.RateRequest.ToJson(CoreCode.RateRequest.All()));
            ToReturn.Headers.Add("Content-Type", "application/json");
            return ToReturn;
            
        }


    }
}
