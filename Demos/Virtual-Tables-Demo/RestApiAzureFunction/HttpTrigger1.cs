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
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

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

        [Function("nmosi")]
        public static async Task<HttpResponseData> nmosi([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "nmosi/{table?}")] HttpRequestData req, FunctionContext context, string table)
        {
            ILogger log = context.GetLogger("nmosi");

            //Say hi
            log.LogInformation("NMOSI endpoint called!");

            //Log the request URL
            log.LogInformation("Logging the call...");
            await ApiCallLogToolkit.UploadApiCallLogAsync(req.Url.ToString());
            log.LogInformation("Call logged.");

            //Write the table
            log.LogInformation("Table provided: <" + table + ">");

            if (table == null)
            {
                log.LogInformation("A table name was not provided.");
                log.LogInformation("Going to return a directory.");

                JObject ToReturn = new JObject();
                ToReturn.Add("@odata.context", "https://nmosi2.azurewebsites.net/nmosi/$metadata");
                
                //Create a list of tables we can provide
                List<JObject> TablesWeCanProvide = new List<JObject>();

                //RateRequest
                JObject rr = new JObject();
                rr.Add("name", "RateRequests");
                rr.Add("kind", "EntitySet");
                rr.Add("url", "RateRequests");
                TablesWeCanProvide.Add(rr);

                //Add The tables we can provide as a JArray
                JArray ToAddArr = JArray.Parse(JsonConvert.SerializeObject(TablesWeCanProvide.ToArray()));
                ToReturn.Add("value", ToAddArr);

                //Return
                HttpResponseData resp = req.CreateResponse();
                resp.StatusCode = HttpStatusCode.OK;
                resp.WriteString(ToReturn.ToString());
                resp.Headers.Add("Content-Type", "application/json");
                return resp;
            }
            else if (table == "RateRequests") //Requesting list of RateRequests
            {
                log.LogInformation("RateRequests table was asked for.");

                //Get the url
                string RequestUrl = req.Url.ToString();
                log.LogInformation("Full request URL: " + RequestUrl);

                //Filtering?
                bool FilteringOnCompanyName = false;
                if (RequestUrl.Contains("filter=startswith(CompanyName,"))
                {
                    FilteringOnCompanyName = true;
                    log.LogInformation("They are trying to filter on company");
                }
                else
                {
                    FilteringOnCompanyName = false;
                    log.LogInformation("They are NOT filtering on company.");
                }

                //Get the RateRequests to include
                List<RateRequest> RateRequestsToReturn = new List<RateRequest>();
                RateRequest[] AllRateRequests = CoreCode.RateRequest.All();
                foreach (RateRequest rr in AllRateRequests)
                {
                    if (FilteringOnCompanyName)
                    {
                        int loc1 = RequestUrl.IndexOf("filter=startswith(CompanyName,");
                        loc1 = RequestUrl.IndexOf(",");
                        int loc2 = RequestUrl.IndexOf(")", loc1 + 1);
                        string CompanyNameSearchParam = RequestUrl.Substring(loc1 + 1, loc2 - loc1 - 1).Replace("'", "").Replace("%27", "");
                        log.LogInformation("CompanyName they are searching for: " + CompanyNameSearchParam);

                        if (rr.CompanyName.ToLower().Contains(CompanyNameSearchParam.ToLower()))
                        {
                            log.LogInformation("Including this one: " + rr.CompanyName);
                            RateRequestsToReturn.Add(rr);
                        }
                        else
                        {
                            log.LogInformation("Not including this one: " + rr.CompanyName);
                        }
                    }   
                    else
                    {
                        RateRequestsToReturn.Add(rr);
                    }
                }

                log.LogInformation("Number of RateRequest records that will be returned: " + RateRequestsToReturn.Count.ToString());

                string JsonToReturn = CoreCode.RateRequest.PrepareODataResponseBody(CoreCode.RateRequest.ToJson(RateRequestsToReturn.ToArray())).ToString();

                HttpResponseData ToReturn = req.CreateResponse();
                ToReturn.StatusCode = HttpStatusCode.OK;
                ToReturn.WriteString(JsonToReturn);
                ToReturn.Headers.Add("Content-Type", "application/json");
                return ToReturn;

            }
            else if (table == "$metadata" || table == "$metadata#RateRequests" || table == "$metadata#RateRequests/$entity") //Metadata
            {
                log.LogInformation("Metadata was requested.");
                HttpClient hc = new HttpClient();
                HttpResponseMessage resp = await hc.GetAsync("https://nmosi.blob.core.windows.net/general/metadata.xml?sp=r&st=2022-01-19T19:47:19Z&se=2027-09-04T02:47:19Z&sv=2020-08-04&sr=b&sig=3xwTVAcUNNMHk%2FVd2y1CyBAaW9hyEFOzfLDgJ3Pwr0c%3D");
                string content = await resp.Content.ReadAsStringAsync();
                HttpResponseData ToResp = req.CreateResponse();
                ToResp.WriteString(content);
                ToResp.Headers.Add("Content-Type", "application/xml");
                return ToResp;
            }
            else if (table.ToLower().Contains("raterequests") && table.ToLower().Contains("(") && table.ToLower().Contains(")")) //selecting of a single record
            {
                //Get the ID of the record
                int loc1 = table.IndexOf("(");
                int loc2 = table.IndexOf(")");
                Guid id = Guid.Parse(table.Substring(loc1 + 1, loc2 - loc1 - 1));

                //Get the fields they are selecting
                string[] SelectFields = new string[]{};
                string url = req.Url.ToString();
                Uri turi = new Uri(url);
                string fselection = HttpUtility.ParseQueryString(turi.Query).Get("$select");
                if (fselection != null)
                {
                    SelectFields = fselection.Split(new string[]{","}, StringSplitOptions.RemoveEmptyEntries);
                }


                //Select it!
                JObject ToReturn = CoreCode.RateRequest.Select(id, SelectFields);

                //Add the odata context - the metadata
                ToReturn.Add("@odata.context", "https://nmosi2.azurewebsites.net/nmosi/$metadata#RateRequests/$entity");


                //Return it
                HttpResponseData ToResp = req.CreateResponse();
                ToResp.WriteString(JsonConvert.SerializeObject(ToReturn));
                ToResp.Headers.Add("Content-Type", "application/json");
                return ToResp;
            }
            else
            {
                HttpResponseData resp = req.CreateResponse();
                resp.StatusCode = HttpStatusCode.BadRequest;
                resp.WriteString("Table '" + table + "' is invalid.");
                return resp;
            }
            
            

            // //Working on it message
            // HttpResponseData WorkingOnIt = req.CreateResponse();
            // WorkingOnIt.StatusCode = HttpStatusCode.Locked;
            // WorkingOnIt.WriteString("Working on this...");
            // return WorkingOnIt;


        }

        [Function("sample")]
        public static async Task<HttpResponseData> sample([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sample/{table?}")] HttpRequestData req, FunctionContext context, string table)
        {
            ILogger log = context.GetLogger("sample");


            if (table == null)
            {
                log.LogInformation("Table not provided. Will provide directory");
                JObject ToReturn = new JObject();
                ToReturn.Add("@odata.context", "https://nmosi2.azurewebsites.net/sample/$metadata");
                JObject jo = new JObject();
                jo.Add("name", "Advertisements");
                jo.Add("kind", "EntitySet");
                jo.Add("url", "Advertisements");
                JArray ToAddJAr = JArray.Parse(JsonConvert.SerializeObject(new JObject[] {jo}));
                ToReturn.Add("value", ToAddJAr);

                HttpResponseData data = req.CreateResponse();
                data.StatusCode = HttpStatusCode.OK;
                data.WriteString(ToReturn.ToString());
                data.Headers.Add("Content-Type", "application/json");
                return data;
            }
            else if (table == "$metadata" || table == "$metadata#Advertisements")
            {
                log.LogInformation("Metadata was requested.");
                HttpClient hc = new HttpClient();
                HttpResponseMessage resp = await hc.GetAsync("https://nmosi.blob.core.windows.net/general/SampleMetadata.xml?sp=r&st=2022-01-20T15:31:30Z&se=2024-10-03T22:31:30Z&sv=2020-08-04&sr=b&sig=0Eq6dQhEic%2FJM4BzPHNtUughlpIcuQesh4jF9uVG%2F68%3D");
                string content = await resp.Content.ReadAsStringAsync();
                HttpResponseData ToResp = req.CreateResponse();
                ToResp.WriteString(content);
                ToResp.Headers.Add("Content-Type", "application/xml");
                return ToResp;
            }
            else if (table == "Advertisements")
            {
                
                //Item 1
                JObject jo1 = new JObject();
                //jo1.Add("@odata.mediaReadLink", "Advertisements(f89dee73-af9f-4cd4-b330-db93c25ff3c7)/$value");
                //jo1.Add("@odata.mediaContentType", "*/*");
                //jo1.Add("@odata.mediaEtag", "\"8zOOKKvgOtptr4gt8IrnapX3jds=\"");
                jo1.Add("ID", Guid.Parse("f89dee73-af9f-4cd4-b330-db93c25ff3c7"));
                jo1.Add("Name", "Old School Lemonade Store, Retro Style");
                jo1.Add("AirDate", DateTimeOffset.UtcNow);

                //Item 1
                JObject jo2 = new JObject();
                //jo2.Add("@odata.mediaReadLink", "Advertisements(db2d2186-1c29-4d1e-88ef-a127f521b9c6)/$value");
                //jo2.Add("@odata.mediaContentType", "*/*");
                jo2.Add("ID", Guid.Parse("db2d2186-1c29-4d1e-88ef-a127f521b9c6"));
                jo2.Add("Name", "Early morning start, need coffee");
                jo2.Add("AirDate", DateTimeOffset.UtcNow);

                JObject ToReturn = new JObject();
                ToReturn.Add("@odata.context", "https://nmosi2.azurewebsites.net/sample/$metadata#Advertisements");
                ToReturn.Add("value", JArray.Parse(JsonConvert.SerializeObject(new JObject[]{jo1, jo2})));

                HttpResponseData resp = req.CreateResponse();
                resp.StatusCode = HttpStatusCode.OK;
                resp.WriteString(ToReturn.ToString());
                resp.Headers.Add("Content-Type", "application/json");
                return resp; 
            }
            else
            {
                HttpResponseData resp = req.CreateResponse();
                resp.StatusCode = HttpStatusCode.BadRequest;
                resp.WriteString("Table '" + table + "' is invalid.");
                return resp;
            }
        }


    }
}
