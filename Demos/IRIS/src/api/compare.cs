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
    public class compare
    {
        [Function("compare")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {

            //EXAMPLE POST BODY BELOW
            //  {
            //      "testimonies": "[{\"id\":\"dc3d5ade-4610-ee11-8f6d-0022481c8ac0\"},{\"id\": \"9d9d9dd8-a110-ee11-8f6d-0022481c8ac0\"}]"
            //  }

            //Parse body
            StreamReader sr = new StreamReader(req.Body);
            string content = await sr.ReadToEndAsync();
            JObject jo = JObject.Parse(content);

            //Look for property
            JProperty? prop_testimonies = jo.Property("testimonies");
            if (prop_testimonies != null)
            {
                //Collect the ids
                string testimonies_json = prop_testimonies.Value.ToString();
                JArray testimonies_records = JArray.Parse(testimonies_json);
                List<Guid> testimony_ids = new List<Guid>();
                foreach (JObject tr in testimonies_records)
                {
                    JProperty? prop_id = tr.Property("id");
                    if (prop_id != null)
                    {
                        testimony_ids.Add(Guid.Parse(prop_id.Value.ToString()));
                    }
                }

                //Collect each testimony
                CdsService cds = await InvestigationTools.CdsAuthAsync();
                CaseTestimony[] CaseTestimonies = await InvestigationTools.RetrieveAndPrepareTestimoniesAsync(testimony_ids.ToArray(), cds);

                //Ask GPT to compare.
                string response = await InvestigationTools.CompareTestimoniesAsync(CaseTestimonies);

                //Response object
                JObject r = new JObject();
                r.Add("response", response);
                

                //Respond
                HttpResponseData resp = req.CreateResponse();
                resp.StatusCode = HttpStatusCode.OK;
                resp.WriteString(r.ToString());
                resp.Headers.Add("Content-Type", "application/json");
                return resp;
            }
            else
            {
                HttpResponseData resp = req.CreateResponse();
                resp.StatusCode = HttpStatusCode.BadRequest;
                resp.WriteString("You must provide property 'testimonies' in the body object.");
                return resp;
            }
        }
    }
}