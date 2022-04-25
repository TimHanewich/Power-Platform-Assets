using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings;

namespace RequestForLeaveEmployeeAggregateCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateAggregates();           
        }

        public class Person
        {
            public string Name {get; set;}
            public int Age {get; set;}
        }

        static void CreateAggregates()
        {
            string env_url = "https://org89df2b3b.crm.dynamics.com/"; //The URL of your CDS environment.
            string client_id = "a5ad2463-0a21-4e98-bae8-dc36bdf432ba"; //From your registered application in the Azure portal.

            string baseURL = "https://login.microsoftonline.com/common/oauth2/authorize";
            string param1 = "resource=" + env_url;
            string param2 = "client_id=" + client_id;
            string param3 = "response_type=token"; //We are asking for an access token.
            string AUTH_URL = baseURL + "?" + param1 + "&" + param2 + "&" + param3;

            //Have the user go to this endpoint in a browser and authenticate here
            Console.WriteLine("Please navigate to this URL in a browser and authenticate with an account that has permission to access the environment '" + env_url + "'");
            Console.WriteLine(AUTH_URL);

            //After the user authenticates into that URL, it will redirect them to the webpage that you have set as the Redirect URL in the registered app in the Azure Portal.
            //The access token will be in the URL header as a paramater.
            //In an actual production app, you will want that redirect endpoint to be one of your API's that then receives that access token as an input.
            //And that will be your access token that we will use moving forward.
            //Since we did not do that (this is just a demo), we will ask the user to copy + paste that redirect address into this app and parse the token that we need out of it.

            //Ask for the address
            Console.WriteLine();
            Console.WriteLine("After authenticating, please copy + paste the URL from the address bar that you are redirected to");
            Console.WriteLine("Paste below:");
            string redirected_to_address = Console.ReadLine();
            Console.WriteLine();

            //Parse the access token out
            int loc1 = redirected_to_address.IndexOf("access_token");
            loc1 = redirected_to_address.IndexOf("=", loc1 + 1);
            int loc2 = redirected_to_address.IndexOf("&", loc1 + 1);
            string access_token = redirected_to_address.Substring(loc1 + 1, loc2 - loc1 - 1);

            
            //string access_token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6ImppYk5ia0ZTU2JteFBZck45Q0ZxUms0SzRndyIsImtpZCI6ImppYk5ia0ZTU2JteFBZck45Q0ZxUms0SzRndyJ9.eyJhdWQiOiJodHRwczovL29yZzE1OWEyYWRjMC5jcm0uZHluYW1pY3MuY29tIiwiaXNzIjoiaHR0cHM6Ly9zdHMud2luZG93cy5uZXQvMzI2ZjA4MDYtOTU1YS00MzliLWFjMTAtN2U1Nzg2YTI0YTk1LyIsImlhdCI6MTU5OTI0Nzc3MSwibmJmIjoxNTk5MjQ3NzcxLCJleHAiOjE1OTkyNTE2NzEsImFjciI6IjEiLCJhaW8iOiJFMkJnWUZpL3JhRmtEWDlTL052SWl3Nm1kVjJoRnB4K2s4VlBQWngvckNOaFl3QlBLeWNBIiwiYW1yIjpbInB3ZCJdLCJhcHBpZCI6ImE1YWQyNDYzLTBhMjEtNGU5OC1iYWU4LWRjMzZiZGY0MzJiYSIsImFwcGlkYWNyIjoiMCIsImZhbWlseV9uYW1lIjoiQWRtaW4iLCJnaXZlbl9uYW1lIjoiU3lzdGVtIiwiaXBhZGRyIjoiMTMxLjEwNy4xNjAuMTY3IiwibmFtZSI6IlN5c3RlbSBBZG1pbiIsIm9pZCI6IjVlMGJhOTJjLWQ5ZWUtNDlkZS04ZTc5LWRkMmVlNTRjNDczMyIsInB1aWQiOiIxMDAzMjAwMDY2ODk2MDJBIiwicmgiOiIwLkFUY0FCZ2h2TWxxVm0wT3NFSDVYaHFKS2xXTWtyYVVoQ3BoT3V1amNOcjMwTXJvM0FOUS4iLCJzY3AiOiJ1c2VyX2ltcGVyc29uYXRpb24iLCJzdWIiOiJGeTFVQlFHeWZnYmwwQ1ljc0NadFUzb2RCdENNSXlxeWx0MGZWZmZWamprIiwidGlkIjoiMzI2ZjA4MDYtOTU1YS00MzliLWFjMTAtN2U1Nzg2YTI0YTk1IiwidW5pcXVlX25hbWUiOiJzeXN0ZW1hZG1pbkB0aGFuZXdpY2gub25taWNyb3NvZnQuY29tIiwidXBuIjoic3lzdGVtYWRtaW5AdGhhbmV3aWNoLm9ubWljcm9zb2Z0LmNvbSIsInV0aSI6ImhGTm5DZmxsVVVDUTZNeGRTTEFsQUEiLCJ2ZXIiOiIxLjAifQ.pMJ_NxYLc7_mv0nttpbX4byYcSgdCMvkUQTzoNU2DonJ4nLbjdog1OvYt6du0-O6K90_mpLB75uwiK6QokIMR4XfPLFqri2pm4-S4iKEyKMuwyC37YVFMaQVZcFcMLdvPJBlOnbTKjkVVWC8xfL-VRrWdYmGxG4fYmP4j823WTxA3navnqSnJ2dPcvQo4gWOTkhMr88zUOx1tJv-UvZTYVUWZukHaDvdpuo4ybqrTLFKjWuYFLFODipwsz1ir4KHFIsH8FijNCornbV1DY0Z1xDcacQJFjpCN55o1qJNmq7qkBWGFQw3RHBpcTka2cgu_EvyFobaBxAzV1UhUjng4A";

            //Construct the web api URl - This will be the endpoint that we use for all of our API calls to the CDS
            //You can also navigate to this URL to see a list of all entity setters
            string web_api_url = env_url + "/api/data/v9.0/";

            CdsHelper helper = new CdsHelper(web_api_url, access_token);
            HttpClient hc = new HttpClient();

            #region "TESTING"

            // string ug = "019b61af-2f65-4d22-8b77-df2429027e14";

            // float t1 = helper.CountHoursUsedDuringMonth(ug, 9);
            // float t2 = helper.CountHoursUsedDuringMonth(ug, 10);
            // float t3 = helper.CountHoursUsedDuringMonth(ug, 11);
            // float t4 = helper.CountHoursUsedDuringMonth(ug, 12);
            // Console.WriteLine(t1.ToString());
            // Console.WriteLine(t2.ToString());
            // Console.WriteLine(t3.ToString());
            // Console.WriteLine(t4.ToString());
            // Console.WriteLine("Kill now!");
            // Console.ReadLine();
            // Console.ReadLine();
            // Console.ReadLine();
            // Console.ReadLine();
            // Console.ReadLine();

            #endregion

            #region "Step 1 - Delete all of the current employee aggregates"

            //Get all of the GUID's of the current aggregates
            JObject[] current_employee_aggs = helper.GetEntities("rfl_employeeleavereportingaggregates");
            

            //Parse out all of the guids
            List<string> Agg_Guids = new List<string>();
            foreach (JObject jo in current_employee_aggs)
            {
                Agg_Guids.Add(jo["rfl_employeeleavereportingaggregateid"].ToString());
            }


            //Delete each of them
            foreach (string g in Agg_Guids)
            {
                Console.WriteLine("Deleting report " + g + " ...");
                string endpoint = "rfl_employeeleavereportingaggregates(" + g + ")";
                HttpRequestMessage reqmsgdel = new HttpRequestMessage();
                reqmsgdel.Method = HttpMethod.Delete;
                reqmsgdel.RequestUri = new Uri(web_api_url + endpoint);
                reqmsgdel.Headers.Add("Authorization", "Bearer " + access_token);
                hc.SendAsync(reqmsgdel).Wait();
            }
            

            #endregion
        
            #region "Step 2 - Get the GUID of every user"

            HttpRequestMessage requ = new HttpRequestMessage();
            requ.Method = HttpMethod.Get;
            requ.Headers.Add("Authorization", "Bearer " + access_token);
            requ.RequestUri = new Uri(web_api_url + "systemusers");

            
            HttpResponseMessage respmsg = hc.SendAsync(requ).Result;
            string contentj = respmsg.Content.ReadAsStringAsync().Result;
            JObject systemusersreturn = JObject.Parse(contentj);
            List<string> UserGuids = new List<string>();
            Console.WriteLine("Getting GUID of every user!");
            foreach (JObject joo in systemusersreturn["value"])
            {
                string this_user_guid = joo["systemuserid"].ToString();
                Console.WriteLine("Found user " + this_user_guid);
                UserGuids.Add(this_user_guid);
            }
            

            #endregion
        
            #region "Step 3 - Get the data that we will use constantly"

            //Get leave requests
            HttpRequestMessage reqmsg = new HttpRequestMessage();
            reqmsg.Method = HttpMethod.Get;
            reqmsg.RequestUri = new Uri(web_api_url + "rfl_leaverequests");
            reqmsg.Headers.Add("Authorization", "Bearer " + access_token);
            HttpResponseMessage resp_lrs = hc.SendAsync(reqmsg).Result;
            string lrs_content = resp_lrs.Content.ReadAsStringAsync().Result;
            JObject leave_requests = JObject.Parse(lrs_content);

            //Get all of the leave times for this request
            HttpRequestMessage req_lt = new HttpRequestMessage();
            req_lt.Method = HttpMethod.Get;
            req_lt.RequestUri = new Uri(web_api_url + "rfl_leavetimes");
            req_lt.Headers.Add("Authorization", "Bearer " + access_token);
            HttpResponseMessage resp_lt = hc.SendAsync(req_lt).Result;
            string cont_lt = resp_lt.Content.ReadAsStringAsync().Result;
            JObject leave_times = JObject.Parse(cont_lt);

            #endregion

            #region "Step 4 - Create all of the aggregates for each user"

            foreach (string user_guid in UserGuids)
            {
                
                //Get the user
                HttpRequestMessage reqmsg_user = new HttpRequestMessage();
                reqmsg_user.Method = HttpMethod.Get;
                reqmsg_user.RequestUri = new Uri(web_api_url + "systemusers(" + user_guid + ")");
                reqmsg_user.Headers.Add("Authorization", "Bearer " + access_token);
                HttpResponseMessage user_rspmsg = hc.SendAsync(reqmsg_user).Result;
                string usercontent = user_rspmsg.Content.ReadAsStringAsync().Result;
                
                
                //Get their data
                JObject this_user = JObject.Parse(usercontent);
                string FINAL_FullName = this_user["fullname"].ToString();
                string FINAL_PictureURL = this_user["rfl_fillinprofilepicture"].ToString();
                
                //Get the total alloted time off
                string allotted_hours = this_user["rfl_timeoffavailable"].ToString();
                float alloted_hours_fl = 0;
                if (allotted_hours != null)
                {
                    if (allotted_hours != "null" && allotted_hours != "")
                    {
                        try
                        {
                            alloted_hours_fl = Convert.ToSingle(allotted_hours);
                        }
                        catch
                        {
                            alloted_hours_fl = 0;
                        }
                    }
                }

                //Get the # of hours the employee is taking off EVER (doesn't matter when);
                //Go through all of the leave requests. Find the leave requests this user owns. If it is approved, go through and count the number of hours for each one
                float HoursOffEver = 0;
                foreach (JObject lr in leave_requests["value"])
                {
                    if (lr["_ownerid_value"].ToString() == user_guid) //This user owns this leave request
                    {
                        if (lr["rfl_requestapprovalstatus"] != null)
                        {
                            if ((int)lr["rfl_requestapprovalstatus"] == 228740001) //If it is approved, lets count up the # of hours in each leave time
                            {
                                
                                //What is this leave requests GUID?
                                string this_leave_requests_guid = lr["rfl_leaverequestid"].ToString();

                                //Go through all of them. If they are for this leave request, add up the hours
                                float num_of_hours_on_this_request = 0;
                                foreach (JObject ilt in leave_times["value"])
                                {

                                    if (ilt["_rfl_parentleaverequest_value"] != null)
                                    {
                                        string parent_lr = ilt["_rfl_parentleaverequest_value"].ToString();
                                        string this_lr_guid = lr["rfl_leaverequestid"].ToString();
                                        if (parent_lr == this_lr_guid)
                                        {
                                            try
                                            {
                                                num_of_hours_on_this_request = num_of_hours_on_this_request + (float)ilt["rfl_hoursoff"];
                                            }
                                            catch
                                            {

                                            }                                      
                                        }
                                    }

                                }

                                //Add the # of hours for this request to the total
                                HoursOffEver = HoursOffEver + num_of_hours_on_this_request;
                            }
                        }
                        
                    }
                }
                
                //Get the remaining time off (this would be the allotted time off (that we have up there) minus the time already approved off)
                float FINAL_RemainingAvailableHoursOff = Math.Max(0, alloted_hours_fl - HoursOffEver);
              

                //Get the 1, 2, 3, and 4 month out hours used amounts
                float FINAL_HoursOffThisMonth = Math.Max(0, helper.CountHoursUsedDuringMonth(user_guid, DateTime.Now.Month));
                float FINAL_Plus1Month = Math.Max(0, helper.CountHoursUsedDuringMonth(user_guid, DateTime.Now.Month + 1));
                float FINAL_Plus2Month = Math.Max(0, helper.CountHoursUsedDuringMonth(user_guid, DateTime.Now.Month + 2));
                float FINAL_Plus3Month = Math.Max(0, helper.CountHoursUsedDuringMonth(user_guid, DateTime.Now.Month + 3));
                float FINAL_Plus4Month = Math.Max(0, helper.CountHoursUsedDuringMonth(user_guid, DateTime.Now.Month + 4));
                float FINAL_Plus5Month = Math.Max(0, helper.CountHoursUsedDuringMonth(user_guid, DateTime.Now.Month + 5));


                //NOW... the moment we have all been waiting for... create the report aggregate
                JObject ReturnAgg = new JObject();
                ReturnAgg["rfl_employeename"] = FINAL_FullName;
                ReturnAgg["rfl_employeepictureurl"] = FINAL_PictureURL;
                ReturnAgg["rfl_hoursoffthismonth"] = FINAL_HoursOffThisMonth;
                ReturnAgg["rfl_remaininghoursoffavailable"] = FINAL_RemainingAvailableHoursOff;
                ReturnAgg["rfl_plus1monthhoursused"] = FINAL_Plus1Month;
                ReturnAgg["rfl_plus2monthhoursused"] = FINAL_Plus2Month;
                ReturnAgg["rfl_plus3monthhoursused"] = FINAL_Plus3Month;
                ReturnAgg["rfl_plus4monthhoursused"] = FINAL_Plus4Month;
                ReturnAgg["rfl_plus5monthhoursused"] = FINAL_Plus5Month;
                HttpRequestMessage req_create = new HttpRequestMessage();
                req_create.Method = HttpMethod.Post;
                req_create.RequestUri = new Uri(web_api_url + "rfl_employeeleavereportingaggregates");
                req_create.Content = new StringContent(ReturnAgg.ToString(), Encoding.UTF8, "application/json");
                req_create.Headers.Add("Authorization", "Bearer " + access_token);

                //Actually post it
                Console.Write("Creating for " + FINAL_FullName + "... ");
                HttpResponseMessage frm = hc.SendAsync(req_create).Result;
                Console.WriteLine(frm.StatusCode.ToString());
                if (frm.StatusCode != HttpStatusCode.NoContent)
                {
                    Console.WriteLine("Failure msg content: " + frm.Content.ReadAsStringAsync().Result);
                }
            }

            #endregion

        }
    
        public class CdsHelper
        {
            private string AccessToken;
            private string BaseWebApiUrl;

            public CdsHelper(string base_web_api_url, string access_token)
            {
                AccessToken = access_token;
                BaseWebApiUrl = base_web_api_url;
            }

            public JObject GetEntity(string setter_name, string obj_guid)
            {
                HttpClient hc = new HttpClient();
                HttpRequestMessage msg = new HttpRequestMessage();
                msg.Method = HttpMethod.Get;
                msg.RequestUri = new Uri(BaseWebApiUrl + setter_name + "(" + obj_guid + ")");
                msg.Headers.Add("Authorization", "Bearer " + AccessToken);
                HttpResponseMessage resp = hc.SendAsync(msg).Result;
                string cont = resp.Content.ReadAsStringAsync().Result;
                JObject ToReturn = JObject.Parse(cont);
                return ToReturn;
            }

            public JObject[] GetEntities(string setter_name)
            {
                HttpClient hc = new HttpClient();
                HttpRequestMessage msg = new HttpRequestMessage();
                msg.Method = HttpMethod.Get;
                msg.RequestUri = new Uri(BaseWebApiUrl + setter_name);
                msg.Headers.Add("Authorization", "Bearer " + AccessToken);
                HttpResponseMessage resp = hc.SendAsync(msg).Result;
                string cont = resp.Content.ReadAsStringAsync().Result;
                JObject obbb = JObject.Parse(cont);
                List<JObject> ToReturn = new List<JObject>();
                foreach (JObject oo in obbb["value"])
                {
                    ToReturn.Add(oo);
                }
                return ToReturn.ToArray();
            }


            public float CountHoursUsedDuringMonth(string user_guid, int month)
            {
                float ToReturn = 0;

                //Get the things we will use a lot
                JObject[] leave_requests = GetEntities("rfl_leaverequests");
                JObject[] leave_times = GetEntities("rfl_leavetimes");

                foreach (JObject lr in leave_requests)
                {
                    if (lr["_ownerid_value"].ToString() == user_guid) //If we own it
                    {
                        if ((int)lr["rfl_requestapprovalstatus"] == 228740001) //if it is approved
                        {
                            foreach (JObject lt in leave_times)
                            {
                                if (lt["_rfl_parentleaverequest_value"].ToString() == lr["rfl_leaverequestid"].ToString()) //If this is for that specific leave request
                                {
                                    DateTime lt_startdate = (DateTime)lt["rfl_startdatetime"];
                                    if (lt_startdate.Month == month)
                                    {
                                        ToReturn = ToReturn + (float)lt["rfl_hoursoff"];
                                    }
                                }
                                
                            }
                        }
                    }
                }

                return ToReturn;
            }


        }
    
    }
}
