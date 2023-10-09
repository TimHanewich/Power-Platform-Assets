using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using  System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace PSJ
{
    public class SimpleDALLE
    {
        private string _generate_url;
        private string _api_key;
        
        public SimpleDALLE(string generate_url, string api_key)
        {
            _generate_url = generate_url;
            _api_key = api_key;
        }

        public async Task<string[]> GenerateAsBase64Async(string prompt, int count, string size = "512x512")
        {
            //Construct request
            JObject bod = new JObject();
            bod.Add("prompt", prompt);
            bod.Add("size", size);
            bod.Add("n", count);

            //Post request to generate
            HttpRequestMessage req = PrepareReqMsg();
            req.RequestUri = new Uri(_generate_url);
            req.Method = HttpMethod.Post;
            req.Content = new StringContent(bod.ToString(), System.Text.Encoding.UTF8, "application/json");

            //Post
            HttpClient hc = new HttpClient();
            HttpResponseMessage resp = await hc.SendAsync(req);
            string content = await resp.Content.ReadAsStringAsync();
            if (resp.StatusCode != HttpStatusCode.Accepted)
            {
                throw new Exception("POST request to generate using DALLE failed with code '" + resp.StatusCode.ToString() + "': " + content);
            }
            
            //Get the operation loction header
            string operation_location = "";
            try
            {
                IEnumerable<string> values = resp.Headers.GetValues("operation-location");
                foreach (string s in values)
                {
                    operation_location = s;
                }
            }
            catch
            {
                throw new Exception("Header 'operation-location' not found in generation request response.");
            }
            //Console.WriteLine("Operation location: " + operation_location);


            //Collect URLS to collect images (generations)
            List<string> urls = new List<string>();
            while (urls.Count == 0)
            {
                await Task.Delay(2000);

                //Get list of generations
                HttpRequestMessage reqg = PrepareReqMsg();
                reqg.Method = HttpMethod.Get;
                reqg.RequestUri = new Uri(operation_location);
                HttpResponseMessage respg = await hc.SendAsync(reqg);
                string contentg = await respg.Content.ReadAsStringAsync();
                if (respg.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception("Request to get generated image URLS @ '" + operation_location + "' returned code '" + respg.StatusCode.ToString() + "': " + contentg);
                }    

                //Parse
                JObject g = JObject.Parse(contentg);
                JProperty? prop_status = g.Property("status");    
                if (prop_status != null)
                {
                    //Console.WriteLine("Generation status: " + prop_status.Value.ToString());
                    if (prop_status.Value.ToString() == "succeeded")
                    {
                        JToken? jt_data = g.SelectToken("result.data");
                        if (jt_data != null)
                        {
                            JArray data = (JArray)jt_data;
                            foreach (JObject jourl in data)
                            {
                                JProperty? prop_url = jourl.Property("url");
                                if (prop_url != null)
                                {
                                    urls.Add(prop_url.Value.ToString());
                                }
                            }
                        }
                    }
                }    
            }

            //Collect a list of tasks to run to collect
            List<Task<HttpResponseMessage>> ImageRetrievals = new List<Task<HttpResponseMessage>>();
            foreach (string url in urls)
            {
                ImageRetrievals.Add(hc.GetAsync(url));
            }

            //Get all images
            HttpResponseMessage[] responses = await Task.WhenAll(ImageRetrievals);

            //Get each, all at once
            List<string> ToReturn = new List<string>();
            foreach (HttpResponseMessage respi in responses)
            {
                byte[] imagebytes = await respi.Content.ReadAsByteArrayAsync();
                string b64 = Convert.ToBase64String(imagebytes);
                ToReturn.Add(b64);
            }


            // //Get each image with the url
            // List<string> ToReturn = new List<string>();
            // foreach (string url in urls)
            // {
            //     HttpResponseMessage respi = await hc.GetAsync(url);
            //     byte[] imagebytes = await respi.Content.ReadAsByteArrayAsync();
            //     string b64 = Convert.ToBase64String(imagebytes);
            //     ToReturn.Add(b64);
            // }

            return ToReturn.ToArray();            
        }

        private HttpRequestMessage PrepareReqMsg()
        {
            HttpRequestMessage req = new HttpRequestMessage();
            req.Method = HttpMethod.Get;
            req.Headers.Add("api-key", _api_key);
            return req;
        }


    }
}