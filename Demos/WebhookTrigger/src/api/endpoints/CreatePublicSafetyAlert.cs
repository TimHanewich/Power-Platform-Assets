using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PublicSafety;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace PublicSafetyAPI
{
    public class CreatePublicSafetyAlert
    {
        [Function("newpsa")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            string errmsg = "";
            try
            {
                //Create the record at random
                PublicSafetyAlert psa = PublicSafetyAlert.Random();
                Console.WriteLine("Random PSA made: " + JsonConvert.SerializeObject(psa));

                //Add it to the DB
                Console.WriteLine("Uploading to Database... ");
                MockDatabase db = new MockDatabase();
                await db.AddPublicSafetyAlertAsync(psa);
                
                //Get a list of webhook subscribers... and one by one, inform them of the new public safety alert!
                Console.WriteLine("Getting list of subscribers... ");
                WebhookSubscription[] subs = await db.DownloadWebhookSubscriptionsAsync();
                Console.WriteLine(subs.Length.ToString() + " subscribers!");
                foreach (WebhookSubscription sub in subs)
                {
                    HttpRequestMessage hrm = new HttpRequestMessage();
                    hrm.Method = HttpMethod.Post;
                    hrm.RequestUri = new Uri(sub.Endpoint);
                    hrm.Content = new StringContent(JsonConvert.SerializeObject(psa), System.Text.Encoding.UTF8, "application/json");
                    HttpClient hc = new HttpClient();
                    Console.WriteLine("Notifying subscription '" + sub.Id + "' (" + sub.Endpoint + ")... ");

                    try
                    {
                        HttpResponseMessage subresp = await hc.SendAsync(hrm);
                        Console.WriteLine("'" + sub.Endpoint + "' accepted the message and returned code '" + subresp.StatusCode.ToString() + "'!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Message to '" + sub.Endpoint + "' failed! Msg: " + ex.Message);
                    }
                }

                //Assemble response body... purly for information purposes
                JObject jo = new JObject();
                jo.Add("newRecord", JObject.Parse(JsonConvert.SerializeObject(psa)));
                jo.Add("webhookSubscribersNotifiedCount", subs.Length);
                jo.Add("webhookSubscribersNotified", JArray.Parse(JsonConvert.SerializeObject(subs)));

                //Say 201 created
                HttpResponseData sresp = req.CreateResponse();
                sresp.StatusCode = System.Net.HttpStatusCode.Created;
                sresp.Headers.Add("Content-Type", "application/json");
                await sresp.WriteStringAsync(jo.ToString());
                return sresp;
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