using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PublicSafety;
using System.Net.Http;


namespace PublicSafetyAPI
{
    public class GetPublicSafetyAlert
    {
        [Function("alerts")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "alerts/{cmd?}")] HttpRequestData req, string? cmd)
        {

            string errmsg = "";
            try
            {

                //Is there a cmd?
                if (cmd != null)
                {
                    if (cmd != "")
                    {
                        Console.WriteLine("Command received: '" + cmd + "'");
                        if (cmd.Trim().ToLower() == "clear")
                        {
                            MockDatabase dbt = new MockDatabase();
                            await dbt.ClearPublicSafetyAlertsAsync();

                            HttpResponseData cresp = req.CreateResponse();
                            cresp.StatusCode = System.Net.HttpStatusCode.NoContent;
                            return cresp;
                        }
                        else
                        {
                            HttpResponseData breq = req.CreateResponse();
                            breq.StatusCode = System.Net.HttpStatusCode.BadRequest;
                            await breq.WriteStringAsync("Command '" + cmd + "' not understood.");
                            return breq;
                        }
                    }
                }

                //Get data
                MockDatabase db = new MockDatabase();
                PublicSafetyAlert[] alerts = await db.DownloadPublicSafetyAlertsAsync();

                HttpResponseData response = req.CreateResponse();
                response.StatusCode = System.Net.HttpStatusCode.OK;
                response.Headers.Add("Content-Type", "application/json");
                await response.WriteStringAsync(JsonConvert.SerializeObject(alerts), System.Text.Encoding.UTF8);

                return response;
            }
            catch (Exception ex)
            {
                errmsg = ex.Message;
            }

            HttpResponseData resp = req.CreateResponse();
            resp.StatusCode = System.Net.HttpStatusCode.InternalServerError;
            await resp.WriteStringAsync("Failure! Msg: " + errmsg);
            return resp;
            
        }
    }
}