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


    
        #region "Sketch drawing generation"

        public static async Task GenerateSuspectDrawingsAsync(Guid witness_desscription_id)
        {
            CdsService cds = await CdsAuthAsync();

            //Get the witness description
            JObject WitnessDescription = await cds.GetRecordAsync("inv_witnessdescriptions", witness_desscription_id.ToString());

            //Get description
            string description = "";
            JProperty? prop_description = WitnessDescription.Property("inv_description");
            if (prop_description != null)
            {
                description = prop_description.Value.ToString();
            }

            //Get the GUID of the case this description points to
            Guid CaseId = Guid.Empty;
            JProperty? prop_relatedto = WitnessDescription.Property("inv_relatedto_value");
            if (prop_relatedto != null)
            {
                CaseId = Guid.Parse(prop_relatedto.Value.ToString());
            }

            //Construct the prompt
            string PROMPT = "Composite pencil sketch (suspect drawing) like those used in investigations for a suspect with the following description: " + description;

            //Generate
            DALLECredentialsProvider cp = new DALLECredentialsProvider();
            SimpleDALLE dalle = new SimpleDALLE(cp.GenerateUrl, cp.ApiKey);
            string[] imageb64s = await dalle.GenerateAsBase64Async(PROMPT, 4, "512x512");

            //Upload each suspect drawing
            List<Task> Uploads = new List<Task>();
            foreach (string b64 in imageb64s)
            {
                //Construct what to post
                JObject body = new JObject();
                body.Add("inv_ForCase@odata.bind", "inv_cases(" + CaseId.ToString() + ")"); //Connect to case
                body.Add("inv_BasedOnDescription@odata.bind", "inv_witnessdescriptions(" + witness_desscription_id.ToString() + ")"); //Connect to the witness description it came from
                body.Add("inv_drawingstatus", 826570000); //drawing is proposed, not accepted or rejected (must be reviewed)
                body.Add("inv_drawing", b64); //The image content

                Uploads.Add(cds.CreateRecordAsync("inv_suspectdrawings", body.ToString()));
            }   

            //Run uploads at the same time
            await Task.WhenAll(Uploads);
        }

        #endregion




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