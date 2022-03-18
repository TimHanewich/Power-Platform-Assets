using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Web;
using System;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using TimHanewich.Cds;
using TimHanewich.Cds.AdvancedRead;
using CoreLibrary;

namespace DOCAI
{
    public static class HttpTrigger1
    {
        // [Function("HttpTrigger1")]
        // public static HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
        //     FunctionContext executionContext)
        // {
        //     var logger = executionContext.GetLogger("HttpTrigger1");
        //     logger.LogInformation("C# HTTP trigger function processed a request.");

        //     var response = req.CreateResponse(HttpStatusCode.OK);
        //     response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        //     response.WriteString("Welcome to Azure Functions!");

        //     return response;
        // }

        //Detects faces in a picture and saves them to CDS.
        [Function("detect")]
        public static HttpResponseData detect([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req, FunctionContext context)
        {
            //Parameters:
            //pod = the GUID of the pod that the picture was taken in (if it is a pod)
            //cell = the GUID of the cell that the picture was taken in (if it is a cell)
            
            //Body: the full stream of the image itself

            //Get the params
            // Guid? pod = null;
            // Guid? cell = null;
            // NameValueCollection nvc = HttpUtility.ParseQueryString(req.Url.Query);
            // string ip_pod = nvc.Get("pod");
            // if (ip_pod != null)
            // {
            //     pod = Guid.Parse(ip_pod);
            // }
            // string ip_cell = nvc.Get("cell");
            // if (ip_cell != null)
            // {
            //     cell = Guid.Parse(ip_cell);
            // }
            
            
            // //Get the image
            // Stream img = req.Body;

            //Return not available
            HttpResponseData response = req.CreateResponse();
            response.StatusCode = HttpStatusCode.ServiceUnavailable;
            return response;




        }

        //Returns a map
        [Function("map")]
        public static async Task<HttpResponseData> map([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req, FunctionContext context)
        {
            //Params: time - in UNIX time stamp format, the time you are collecting a map for

            //Get time param
            NameValueCollection nvc = HttpUtility.ParseQueryString(req.Url.Query);
            DateTime? BEFORE = null;
            string ip_time = nvc.Get("time");
            if (ip_time != null)
            {
                double unix = Double.Parse(ip_time);
                BEFORE = UnixTimeStampToDateTime(unix);
            }

            //Authenticate CDS service
            CdsService service = await FaceAuthenticator.AuthenticateCDSAsync();

            //Get the map
            JObject ToReturn = await CoreLibrary.Program.PrepareMapAsync(service, BEFORE);
            
            //Return the map
            HttpResponseData response = req.CreateResponse();
            response.StatusCode = HttpStatusCode.OK;
            response.Headers.Add("Content-Type", "application/json");
            response.WriteString(ToReturn.ToString());
            return response;
        }


        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

    }
}
