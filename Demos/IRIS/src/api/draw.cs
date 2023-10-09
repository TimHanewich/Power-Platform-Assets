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
    public class draw
    {
        [Function("draw")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            //EXAMPLE POST BODY BELOW
            // {
            //    "id": "4081651b-b266-ee11-8def-001dd80bf6ae" //GUID of the witness testimony
            //    "count": 4 //how many images to generate
            // }

            //Parse body
            StreamReader sr = new StreamReader(req.Body);
            string content = await sr.ReadToEndAsync();
            JObject jo = JObject.Parse(content);

            //Get id
            Guid id = Guid.Empty;
            JProperty? prop_id = jo.Property("id");
            if (prop_id != null)
            {
                try
                {
                    id = Guid.Parse(prop_id.Value.ToString());
                }
                catch
                {
                    HttpResponseData resp = req.CreateResponse();
                    resp.StatusCode = HttpStatusCode.BadRequest;
                    resp.WriteString("Property 'id' was not in GUID format.");
                    return resp;
                }
            }
            else
            {
                HttpResponseData resp = req.CreateResponse();
                resp.StatusCode = HttpStatusCode.BadRequest;
                resp.WriteString("Property 'id' was not included in body.");
                return resp;
            }

            //Get count
            int count = 4;
            JProperty? prop_count = jo.Property("count");
            if (prop_count != null)
            {
                try
                {
                    count = Convert.ToInt32(prop_count.Value.ToString());
                }
                catch
                {
                    HttpResponseData resp = req.CreateResponse();
                    resp.StatusCode = HttpStatusCode.BadRequest;
                    resp.WriteString("Property 'count' was not an integer.");
                    return resp;
                }
            }
            else
            {
                HttpResponseData resp = req.CreateResponse();
                resp.StatusCode = HttpStatusCode.BadRequest;
                resp.WriteString("Property 'count' was not included in body.");
                return resp;
            }

            //Generate!
            try
            {
                await InvestigationTools.GenerateSuspectDrawingsAsync(id, count);

                //Return 200OK
                HttpResponseData resp = req.CreateResponse();
                resp.StatusCode = HttpStatusCode.OK;
                return resp;
            }
            catch (Exception ex)
            {
                HttpResponseData resp = req.CreateResponse();
                resp.StatusCode = HttpStatusCode.InternalServerError;
                resp.WriteString("Error while generating. Msg: " + ex.Message);
                return resp;
            }
        }
    }
}