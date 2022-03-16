using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using TimHanewich.Cds;

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

        public static async Task<CdsService> AuthenticateCDSAsync()
        {
            CdsAuthenticator auth = new CdsAuthenticator();
            auth.Username = "USERNAME";
            auth.Password = "PASSWORD";
            auth.Resource = "https://org007274b9.crm.dynamics.com/";
            auth.ClientId = Guid.Parse("51f81489-12ee-4a9e-aaae-a2591f45987d");
            await auth.GetAccessTokenAsync();

            CdsService service = new CdsService(auth.Resource, auth.AccessToken);
            return service;
        }
    }
}