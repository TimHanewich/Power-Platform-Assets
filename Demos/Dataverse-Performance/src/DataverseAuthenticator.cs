using System;
using System.Threading.Tasks;
using TimHanewich.Cds;

namespace DataversePerformance
{
    public class DataverseAuthenticator
    {
        public static async Task<string> GetAccessTokenAsync()
        {
            CdsAuthenticator auth = new CdsAuthenticator();
            auth.Username = "";
            auth.Password = "";
            auth.ClientId = Guid.Parse("51f81489-12ee-4a9e-aaae-a2591f45987d");
            auth.Resource = "https://orgde82f7a5.crm.dynamics.com/";
            await auth.GetAccessTokenAsync();
            return auth.AccessToken;
        }
    }
}