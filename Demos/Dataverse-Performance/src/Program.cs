using System;
using System.Threading.Tasks;
using TimHanewich.Cds;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Data.Sql;
using System.Data.SqlClient;
using TimHanewich.Csv;
using Newtonsoft.Json.Linq;

namespace DataversePerformance
{
    public class Program
    {

        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "cds")
                {
                    PerformDataverseUploadAsync().Wait();
                }
                else if (args[0] == "cds-single")
                {
                    string env_url = DataverseAuthenticator.GetCdsAuthenticator().Resource;
                    CdsAuthenticator auth = DataverseAuthenticator.GetCdsAuthenticator();
                    auth.GetAccessTokenAsync().Wait();
                    CdsService cds = new CdsService(env_url, auth.AccessToken);

                    Contact[] cs = RandomContacts(1);

                    cds.CreateRecordAsync("contacts", cs[0].ToDataversePayload().ToString()).Wait();
                }
                else if (args[0] == "sql")
                {
                    PerformSqlUploadAsync().Wait();
                }
                else if (args[0] == "cds-auth")
                {
                    CdsAuthenticator auth = DataverseAuthenticator.GetCdsAuthenticator();
                    auth.GetAccessTokenAsync().Wait();
                    Console.WriteLine(auth.AccessToken);
                }
                else if (args[0] == "csv")
                {
                    Console.Write("Dump to file: ");
                    string? FilePath = Console.ReadLine();
                    if (FilePath != null)
                    {
                        FilePath = FilePath.Replace("\"", "");
                        
                        Console.Write("Generating contacts... ");
                        Contact[] contacts = RandomContacts(500);
                        Console.WriteLine("Generated");

                        //Put into array
                        Console.WriteLine("Placing into array... ");
                        JArray ja = new JArray();
                        for (int t = 0; t < contacts.Length; t++)
                        {
                            Console.WriteLine("Placing into array # " + t.ToString("#,##0") + " / " + contacts.Length.ToString("#,##0"));
                            ja.Add(contacts[t].ToDataversePayload());
                        }
                        Console.WriteLine("All in array");

                        //Write to string
                        Console.Write("Converting to CSV... ");
                        CsvFile csv = CsvToolkit.JsonToCsv(ja);
                        Console.WriteLine("Converted");

                        //Write to file
                        Console.Write("Writing to file... ");
                        System.IO.File.WriteAllText(FilePath, csv.GenerateAsCsvFileContent());
                        Console.WriteLine("Saved!");
                    }
                }
                else
                {
                    Console.WriteLine("I do not know that one.");
                }
            }
        }

        public static async Task PerformDataverseUploadAsync()
        {
            string env_url = "https://orgde82f7a5.crm.dynamics.com/";
            CdsAuthenticator auth = DataverseAuthenticator.GetCdsAuthenticator();
            await auth.GetAccessTokenAsync();
            CdsService cds = new CdsService(env_url, auth.AccessToken);


            int cycles = 65000;
            int ConcurrentUploads = 45;

            for (int t = 0; t < cycles; t++)
            {
                float pc = Convert.ToSingle(t) / Convert.ToSingle(cycles);

                //Check if we need to refresh the access token
                TimeSpan TimeRemaining = auth.AccessTokenExpiresUtc - DateTime.UtcNow;
                if (TimeRemaining.TotalMinutes < 3)
                {
                    Console.Write("Refreshing access token... ");
                    await auth.GetAccessTokenAsync();
                    cds = new CdsService(env_url, auth.AccessToken);
                    Console.WriteLine("Refreshed!");
                }

                Console.WriteLine("On cycle # " + t.ToString() + " / " + cycles.ToString("#,##0") + " (" + pc.ToString("#0.0%") + ")");

                Contact[] contactsToUpload = RandomContacts(ConcurrentUploads);

                List<Task> CollectedTasks = new List<Task>();
                foreach (Contact c in contactsToUpload)
                {
                    Task tsk = cds.CreateRecordAsync("contacts", c.ToDataversePayload().ToString());
                    CollectedTasks.Add(tsk);
                }

                //Wait for all to upload
                try
                {
                    Console.Write("Uploading " + CollectedTasks.Count.ToString() + " right now... ");
                    Task.WaitAll(CollectedTasks.ToArray());
                    Console.WriteLine("Success!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("FATAL FAILURE! Msg: " + ex.Message);
                }
            }

        }
        
        public static async Task PerformSqlUploadAsync()
        {
            Contact[] contacts = RandomContacts(5000000);

            SqlConnection sqlcon = new SqlConnection(SqlCredentialsProvider.GetSqlConnectionString());
            await sqlcon.OpenAsync();

            for (int t = 0; t < contacts.Length; t++)
            {
                float pc = Convert.ToSingle(t) / Convert.ToSingle(contacts.Length);

                Contact toupload = contacts[t];
                Console.Write("Uploading # " + (t+1).ToString("#,##0") + " / " + contacts.Length.ToString("#,##0") + " (" + pc.ToString("#0.0%") + ")... ");
                
                SqlCommand sqlcmd = new SqlCommand(toupload.ToSqlInsert(), sqlcon);
                await sqlcmd.ExecuteNonQueryAsync();

                Console.WriteLine("Success!");
            }

            sqlcon.Close();
            
            
        }

        private static Contact[] RandomContacts(int count)
        {
            string[] FirstNames = System.IO.File.ReadAllText(@"C:\Users\tahan\Downloads\Power-Platform-Assets\Demos\Dataverse-Performance\src\FirstNames.txt").Split(Environment.NewLine);
            string[] LastNames = System.IO.File.ReadAllText(@"C:\Users\tahan\Downloads\Power-Platform-Assets\Demos\Dataverse-Performance\src\LastNames.txt").Split(Environment.NewLine);
            string[] cities = System.IO.File.ReadAllText(@"C:\Users\tahan\Downloads\Power-Platform-Assets\Demos\Dataverse-Performance\src\cities.txt").Split(Environment.NewLine);

            Random r = new Random();
            List<Contact> ToReturn = new List<Contact>();
            for (int t = 0; t < count; t++)
            {
                Contact c = new Contact();
                c.FirstName = RandomFromArray(FirstNames);
                c.LastName = RandomFromArray(LastNames);
                c.BirthDate = RandomDOB();
                c.MobilePhone = RandomPhoneNumber();
                c.AddressCity = RandomFromArray(cities);
                c.AddressLatitude = Convert.ToSingle(r.NextDouble());
                c.AddressLongitude = Convert.ToSingle(r.NextDouble());
                c.AnnualIncome = r.Next(30000, 450000);
                ToReturn.Add(c);
            }
            return ToReturn.ToArray();
        }

        private static string RandomFromArray(string[] arr)
        {
            Random r = new Random();
            string selected = arr[r.Next(arr.Length)];
            return selected;
        }

        private static DateTime RandomDOB()
        {
            int year = RandBetween(1950, 2010);
            int month = RandBetween(1, 12);
            int day = RandBetween(1, 27);
            DateTime dob = new DateTime(year, month, day);
            return dob;
        }

        private static long RandomPhoneNumber()
        {
            Random r = new Random();
            long ub = long.Parse("9999999999");
            long tr = r.NextInt64(1000000000, ub);
            return tr;
        }

        private static int RandBetween(int min, int max)
        {
            Random r = new Random();
            int val = r.Next(min, max);
            return val;
        }

        
    }
}