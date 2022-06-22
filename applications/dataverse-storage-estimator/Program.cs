using System;
using TimHanewich.Cds;
using TimHanewich.Cds.Metadata;
using System.Threading;
using System.Threading.Tasks;
using ConsoleVisuals;

namespace DataverseStorageEstimator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            FullProgramAsync().Wait();
        }


        public static async Task FullProgramAsync()
        {
            Console.WriteLine();
            Console.Write("Welcome to the ");
            ConsoleVisualsToolkit.Write("Dataverse Storage Estimator", ConsoleColor.Cyan);
            Console.WriteLine("!");
            Console.WriteLine("Use this tool for estimating how much Dataverse storage capacity will be used by a Table.");
            Console.WriteLine();

            //Collect credentials
            Console.WriteLine("I will collect a few details from you so I can 'peek behind the scenes' at the underlying attributes of your Dataverse table");
            string username = "";
            string password = "";
            string resource = "";
            if (username == "")
            {
                Console.Write("Username: ");
                string? rl = Console.ReadLine();
                if (rl != null)
                {
                    username = rl;
                }
            }
            if (password == "")
            {
                Console.Write("Password: ");
                string? rl = Console.ReadLine();
                if (rl != null)
                {
                    password = rl;
                }
            }
            if (resource == "")
            {
                Console.Write("Next, I will need the ");
                ConsoleVisualsToolkit.Write("Environment URL", ConsoleColor.Cyan);
                Console.WriteLine(" to the Dataverse instance (environment) containing the table you are targeting.");
                Console.Write("Environment URL: ");
                string? rl = Console.ReadLine();
                if (rl != null)
                {
                    resource = rl;
                }
            }
            Console.WriteLine();

            //Authenticate
            Console.Write("Thank you! Authenticating... ");
            CdsAuthenticator auth = new CdsAuthenticator();
            auth.Username = username;
            auth.Password = password;
            auth.Resource = resource;
            auth.ClientId = Guid.Parse("51f81489-12ee-4a9e-aaae-a2591f45987d");
            try
            {
                await auth.GetAccessTokenAsync();
                ConsoleVisualsToolkit.WriteLine("Success!", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                ConsoleVisualsToolkit.WriteLine("Authentication failed: " + ex.Message, ConsoleColor.Red);
                return;
            }

            //Create the service
            CdsService cds = new CdsService(auth.Resource, auth.AccessToken);


            //Get a list of all tables and print them
            Console.Write("Getting tables in environment... ");
            EntityMetadataSummary[] summaries = await cds.GetEntityMetadataSummariesAsync();
            ConsoleTable ct = ConsoleTable.Create("#", "Schema Name", "Display Name");
            for (int t = 0; t < summaries.Length; t++)
            {
                ct.AddRow(t.ToString("#,##0"), summaries[t].SchemaName, summaries[t].DisplayName);
            }
            Console.WriteLine();
            ct.WriteTable();

            
        }



    }
}