using System;

namespace PSJ
{
    public class DALLECredentialsProvider
    {
        public string GenerateUrl {get; set;}
        public string ApiKey {get; set;}

        public DALLECredentialsProvider()
        {
            GenerateUrl = "https://<YOUR OPENAI RESOURCE NAME>.openai.azure.com/openai/images/generations:submit?api-version=2023-06-01-preview";
            ApiKey = "<YOUR API KEY>"; // (a GUID)
        }
    }
}