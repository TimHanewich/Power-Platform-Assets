using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using CoreCode;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Web;
using System;
using System.Collections.Specialized;

namespace VirtualTablesDemo
{
    public static class HttpTrigger1
    {
        
        [Function("RateRequest")]
        public static HttpResponseData RateRequest([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req, FunctionContext context)
        {
            //Params
            //fields - only include certain fields. for example, fields=Id,DateFiled

            ILogger log = context.GetLogger("RateRequest");
            log.LogInformation("RateRequest call received!");

            //Get fields param
            log.LogInformation("The full URL: " + req.Url.ToString());
            NameValueCollection nvc = HttpUtility.ParseQueryString(req.Url.Query);
            string ip_fields = nvc.Get("fields");
            string[] OnlyIncludeFields = null;
            if (ip_fields != null)
            {
                log.LogInformation("Parameter 'fields' was not null: " + ip_fields);
                OnlyIncludeFields = ip_fields.Split(new string[]{","}, StringSplitOptions.None);
            }
            else
            {
                log.LogInformation("Parameter 'fields' was NULL");
            }

            //Get the JSON string to return
            string JsonToReturn = null;
            if (OnlyIncludeFields == null)
            {
                JsonToReturn = CoreCode.RateRequest.PrepareODataResponseBody(CoreCode.RateRequest.ToJson(CoreCode.RateRequest.All())).ToString();
            }
            else
            {
                JsonToReturn = CoreCode.RateRequest.PrepareODataResponseBody(CoreCode.RateRequest.ToJson(CoreCode.RateRequest.All(), OnlyIncludeFields)).ToString();
            }
            
            HttpResponseData ToReturn = req.CreateResponse();
            ToReturn.StatusCode = HttpStatusCode.OK;
            ToReturn.WriteString(JsonToReturn);
            ToReturn.Headers.Add("Content-Type", "application/json");
            return ToReturn;
            
        }


    }
}
