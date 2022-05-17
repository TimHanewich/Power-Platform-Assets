using System;
using System.Threading.Tasks;
using TimHanewich.Cds;
using System.Configuration;

namespace DataversePerformance
{
    public class DataverseAuthenticator
    {
        public static CdsAuthenticator GetCdsAuthenticator()
        {
            string? username = ConfigurationManager.AppSettings.Get("username");
            string? password = ConfigurationManager.AppSettings.Get("password");
            string? url = ConfigurationManager.AppSettings.Get("url");

            if (username != null && password != null && url != null)
            {
                CdsAuthenticator auth = new CdsAuthenticator();
                auth.Username = username;
                auth.Password = password;
                auth.ClientId = Guid.Parse("51f81489-12ee-4a9e-aaae-a2591f45987d");
                auth.Resource = url;
                return auth;
            }
            else
            {
                throw new Exception("Unable to authenticate with Dataverse. The necessary data was not provided.");
            }
        }
    }
}