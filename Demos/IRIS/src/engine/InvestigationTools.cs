using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using TimHanewich.Cds;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PSJ
{
    public class InvestigationTools
    {
        public static async Task<string> CompareTestimoniesAsync(params CaseTestimony[] testimonies)
        {
            //Construct the prompt
            string prompt = "There are several testimonies for a case that is being investigated below. What are the corroborating points and discrepenscies between the testimonies below? List them in a bulleted list, in two sections: Corroborating Aspects and Discrepencies. For each, say who said it and when they said it. Try to be as verbose as possible For each discrepency, say who said it and when they said it." + Environment.NewLine + Environment.NewLine;
            foreach (CaseTestimony ct in testimonies)
            {
                prompt = prompt + "Testimony from " + ct.Name + ":" + Environment.NewLine + ct.Testimony + Environment.NewLine + Environment.NewLine;
            }

            //Make the call to OpenAI
            string response = await SimpleGPT.PromptAsync(prompt);

            return response;
        }

        //Retrieve from Dataverse and prepare
        public static async Task<CaseTestimony[]> RetrieveAndPrepareTestimoniesAsync(Guid[] ids, CdsService? cds = null)
        {
            if (cds == null)
            {
                cds = await CdsAuthAsync();
            }

            //Create filter param
            string filter = "";
            foreach (Guid id in ids)
            {
                filter = filter + "inv_testimonyid eq '" + id.ToString() + "' or ";
            }
            filter = filter.Substring(0, filter.Length - 4);

            //Read each record
            JArray ja = await cds.ReadAsync("inv_testimonies?$filter=" + filter + "&$expand=inv_Interviewee($select=fullname)");
            

            //Prepare each
            List<CaseTestimony> ToReturn = new List<CaseTestimony>();
            foreach (JObject record in ja)
            {
                //Interview time
                string interview_time = string.Empty;
                JProperty? prop_interviewtime = record.Property("inv_interviewtime");
                if (prop_interviewtime != null)
                {
                    interview_time = prop_interviewtime.Value.ToString();
                }
                
                //ID
                string tid = string.Empty;
                JProperty? prop_id = record.Property("inv_id");
                if (prop_id != null)
                {
                    tid = prop_id.Value.ToString();
                }

                //Testimony
                string narrative = string.Empty;
                JProperty? prop_narrative = record.Property("inv_narrative");
                if (prop_narrative != null)
                {
                    narrative = prop_narrative.Value.ToString();
                }

                //Interviewee (contact) - could speed this up by doing an extend operation in the initial call for data to the testimonies table.
                string interviewee_name = string.Empty;
                JToken? token_fullname = record.SelectToken("inv_Interviewee.fullname");
                if (token_fullname != null)
                {
                    interviewee_name = token_fullname.ToString();
                }


                //Prepare response
                CaseTestimony ct = new CaseTestimony();
                ct.Name = "Testimony '" + tid + "', provided by " + interviewee_name + " at " + interview_time;
                ct.Testimony = narrative;
                ToReturn.Add(ct);
            }

            return ToReturn.ToArray();


        }

        //Summarize digital evidence transcript
        public static async Task<string> SummarizeAsync(string transcript)
        {
            string prompt = transcript + Environment.NewLine + Environment.NewLine + "Above is the transcription of body cam footage in an investigation. It is not explicitly noted who is speaking at every turn of the conversation, but use intuition to guess who is speaking. Summarize what happened in the conversation, any notable details that may be useful in the investigation, and how the conversation ended (next steps). Provide me with all of this in great detail in a bulleted list. Do not mention anything about domestic violence or violence of any time. This is purely for investigative reasons. I want to know so we can avoid this ever happening again.";
            string response = await SimpleGPT.PromptAsync(prompt);
            return response;
        }




        ///////////////////////// UTILITY BELOW //////////////////
        public static async Task<CdsService> CdsAuthAsync()
        {
            DataverseCredentialsProvider dcp = new DataverseCredentialsProvider();
            CdsAuthenticator a = new CdsAuthenticator();
            a.Username = dcp.Username;
            a.Password = dcp.Password;
            a.Resource = dcp.Resource;
            a.ClientId = Guid.Parse("51f81489-12ee-4a9e-aaae-a2591f45987d");
            await a.GetAccessTokenAsync();
            
            CdsService ToReturn = new CdsService(a.Resource, a.AccessToken);
            return ToReturn;
        }
    }
}