using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Encodings;
using System.Collections.Generic;

namespace PSJ.AzureSpeechToText
{
    public class Transcription
    {

        public string Self {get; set;}
        public string Files {get; set;}

        public Transcription()
        {
            Self = String.Empty;
            Files = String.Empty;
        }


        public static async Task<Transcription> CreateAsync(string file_download_url)
        {
            // Create the request body
            JObject body = new JObject();
            JArray contentUrls = new JArray();
            contentUrls.Add(file_download_url);
            body.Add("contentUrls", contentUrls);
            body.Add("locale", "en-US");
            body.Add("displayName", "My Transcription " + Guid.NewGuid().ToString().Replace("-",""));

            //Make the request
            HttpRequestMessage req = Toolkit.PrepareRequest();
            req.Method = HttpMethod.Post;
            req.RequestUri = new Uri("https://eastus.api.cognitive.microsoft.com/speechtotext/v3.1/transcriptions");
            req.Content = new StringContent(body.ToString(), Encoding.UTF8, "application/json");

            //Send
            HttpClient hc = new HttpClient();
            HttpResponseMessage response = await hc.SendAsync(req);
            string content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception("New transcription call returned '" + response.StatusCode.ToString() + "'! Msg: " + content);
            }



            Transcription ToReturn = new Transcription();

            //Parse response
            JObject jo = JObject.Parse(content);
            
            //self
            JProperty? prop_self = jo.Property("self");
            if (prop_self != null)
            {
                ToReturn.Self = prop_self.Value.ToString();
            }

            //links
            JToken? files = jo.SelectToken("links.files");
            if (files != null)
            {
                ToReturn.Files = files.ToString();
            }


            return ToReturn;

        }
    
        public async Task<TranscriptionFile[]> GetFilesAsync()
        {
            //Prepare the request
            HttpRequestMessage req = Toolkit.PrepareRequest();
            req.Method = HttpMethod.Get;
            req.RequestUri = new Uri(Files);

            //Make the request
            HttpClient hc = new HttpClient();
            HttpResponseMessage resp = await hc.SendAsync(req);
            string content = await resp.Content.ReadAsStringAsync();
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Request to get transcription files returned code '" + resp.StatusCode.ToString() + "'. Msg: " + content);
            }

            //Parse each
            List<TranscriptionFile> ToReturn = new List<TranscriptionFile>();
            JObject jo = JObject.Parse(content);
            JToken? values = jo.SelectToken("values");
            if (values != null)
            {
                JArray ja_values = (JArray)values;
                foreach (JObject file in ja_values)
                {
                    TranscriptionFile tf = new TranscriptionFile();

                    //self
                    JProperty? prop_self = file.Property("self");
                    if (prop_self != null)
                    {
                        tf.Self = prop_self.Value.ToString();
                    }

                    //name
                    JProperty? prop_name = file.Property("name");
                    if (prop_name != null)
                    {
                        tf.Name = prop_name.Value.ToString();
                    }

                    //kind
                    JProperty? prop_kind = file.Property("kind");
                    if (prop_kind != null)
                    {
                        string kind = prop_kind.Value.ToString();
                        if (kind == "Transcription")
                        {
                            tf.Kind = TranscriptionFileType.Transcription;
                        }
                        else if (kind == "TranscriptionReport")
                        {
                            tf.Kind = TranscriptionFileType.TranscriptionReport;
                        }
                        else
                        {
                            //Throw errror? Not understood type?
                        }
                    }

                    //ContentUrl
                    JToken? contentUrl = file.SelectToken("links.contentUrl");
                    if (contentUrl != null)
                    {
                        tf.ContentUrl = contentUrl.ToString();
                    }

                    ToReturn.Add(tf);
                }

                return ToReturn.ToArray();
            }
            else
            {
                throw new Exception("Unable to find 'values' property in returned transcription files payload.");
            }
        }
    }
}