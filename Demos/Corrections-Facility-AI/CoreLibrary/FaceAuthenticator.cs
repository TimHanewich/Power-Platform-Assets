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
        //CDS SETTINGS!
        public static string CdsEnvironmentUrl = "";
        public static string CdsUsername = "";
        public static string CdsPassword = "";

        //Azure cognitive speech services settings
        public static string AzureSpeechServicesKey = "";
        public static string AzureSpeehServicesRegion = "";

        public static IFaceClient Authenticate()
        {
            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(""));
            client.Endpoint = "";
            return client;
        }

        public static async Task<CdsService> AuthenticateCDSAsync()
        {
            CdsAuthenticator auth = new CdsAuthenticator();
            auth.Username = CdsUsername;
            auth.Password = CdsPassword;
            auth.Resource = CdsEnvironmentUrl;
            auth.ClientId = Guid.Parse("51f81489-12ee-4a9e-aaae-a2591f45987d");
            await auth.GetAccessTokenAsync();

            CdsService service = new CdsService(auth.Resource, auth.AccessToken);
            return service;
        }
    }
}