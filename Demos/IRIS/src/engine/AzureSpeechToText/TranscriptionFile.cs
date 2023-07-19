using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PSJ.AzureSpeechToText
{
    public class TranscriptionFile
    {
        public string Self {get; set;} //URL to the TranscriptionFile record... NOT the actual transcription content itself (text)
        public string Name {get; set;}
        public TranscriptionFileType Kind {get; set;} //i.e. "Transcription" is a transcript, "TranscriptionReport" is a report on that transcript.
        public string ContentUrl {get; set;}

        public TranscriptionFile()
        {
            Self = string.Empty;
            Name = string.Empty;
            ContentUrl = string.Empty;
        }

        public async Task<string> GetTranscriptDisplayTextAsync()
        {
            //Make request
            HttpRequestMessage req = Toolkit.PrepareRequest();
            req.Method = HttpMethod.Get;
            req.RequestUri = new Uri(ContentUrl);
            HttpClient hc = new HttpClient();
            HttpResponseMessage resp = await hc.SendAsync(req);
            string content = await resp.Content.ReadAsStringAsync();
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Call to get transcript content returned code '" + resp.StatusCode.ToString() + "'. Msg: " + content);
            }
            
            //Parse
            JObject jo = JObject.Parse(content);
            JToken? display = jo.SelectToken("combinedRecognizedPhrases[0].display");
            if (display != null)
            {
                return display.ToString();
            }
            else
            {
                throw new Exception("Unable to find property 'display' in returned transcription content.");
            }
        }
    }
}