using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace CoreLibrary
{
    public class FaceAuthenticator
    {
        public static IFaceClient Authenticate()
        {
            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials("b41d3e97004f4c78a30c79596187eb0e"));
            client.Endpoint = "https://testfaceapi20220311.cognitiveservices.azure.com";
            return client;
        }
    }
}