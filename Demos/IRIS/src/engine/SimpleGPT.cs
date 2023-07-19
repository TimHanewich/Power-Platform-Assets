using System;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;

namespace PSJ
{
    public class SimpleGPT
    {
        public static async Task<string> PromptAsync(string prompt)
        {
            string url = "<YOUR AZURE OPENAI ENDPOINT HERE>";
            string api_key = "<YOUR AZURE OPENAI API KEY HERE>";

            HttpClient hc = new HttpClient();
            HttpRequestMessage req = new HttpRequestMessage();
            req.RequestUri = new Uri(url);
            req.Method = HttpMethod.Post;
            req.Headers.Add("api-key", api_key);
            
            JObject jo = new JObject();
            jo.Add("temperature", 0.0);
            jo.Add("prompt", prompt);
            jo.Add("max_tokens", 1850);
            req.Content = new StringContent(jo.ToString(), Encoding.UTF8, "application/json");

            HttpResponseMessage resp = await hc.SendAsync(req);
            string content = await resp.Content.ReadAsStringAsync();
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("API Response form OpenAI returned " + resp.StatusCode.ToString() + ": " + content);
            }
            
            JObject response = JObject.Parse(content);
            JToken? token = response.SelectToken("choices[0].text");
            if (token != null)
            {
                return token.ToString().Trim();
            }
            else
            {
                throw new Exception("Unable to find desired token.");
            }
        }
    }
}