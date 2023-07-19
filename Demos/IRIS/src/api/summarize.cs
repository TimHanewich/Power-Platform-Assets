using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using PSJ;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using TimHanewich.Cds;

namespace PSJAI
{
    public class summarize
    {
        [Function("summarize")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            //EXAMPLE POST BODY BELOW
            // {
            //    "prompt": ""
            // }

            //Parse body
            StreamReader sr = new StreamReader(req.Body);
            string content = await sr.ReadToEndAsync();
            JObject jo = JObject.Parse(content);

            //Look for property
            JProperty? prop_prompt = jo.Property("prompt");
            if (prop_prompt != null)
            {
                //Get the prompt (text)
                string prompt = prop_prompt.Value.ToString();

                //call
                string response = "";
                try
                {
                    response = await InvestigationTools.SummarizeAsync(prompt);
                }
                catch (Exception e)
                {
                    HttpResponseData resp = req.CreateResponse();
                    resp.StatusCode = HttpStatusCode.InternalServerError;
                    resp.WriteString("There was an issue when calling to GPT. Msg: " + e.Message);
                    return resp;
                }

                //Draft return
                JObject rjo = new JObject();
                rjo.Add("response", response);

                //Return
                HttpResponseData s = req.CreateResponse();
                s.StatusCode = HttpStatusCode.OK;
                s.WriteString(rjo.ToString());
                s.Headers.Add("Content-Type", "application/json");
                return s;
            }
            else
            {
                HttpResponseData resp = req.CreateResponse();
                resp.StatusCode = HttpStatusCode.BadRequest;
                resp.WriteString("You must provide property 'prompt' in the body object.");
                return resp;
            }
        }
    }
}