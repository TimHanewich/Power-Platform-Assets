using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PSJ.AzureSpeechToText
{
    public class Toolkit
    {
        public static HttpRequestMessage PrepareRequest()
        {
            HttpRequestMessage req = new HttpRequestMessage();
            req.Headers.Add("Ocp-Apim-Subscription-Key", "<YOUR AZURE SPEECH TO TEXT SUBSCRIPTION KEY HERE>");
            return req;
        }
    }
}