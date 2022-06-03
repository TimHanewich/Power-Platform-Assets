using System;
using System.Threading.Tasks;
using TimHanewich.Cds;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Data.Sql;
using System.Data.SqlClient;
using TimHanewich.Csv;
using Newtonsoft.Json.Linq;
using TimHanewich.MicrosoftGraphHelper;
using TimHanewich.MicrosoftGraphHelper.Sharepoint;
using System.Configuration;
using TimHanewich.Cds.AdvancedRead;

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
                else if (args[0] == "sp")
                {
                    PerformSharepointUploadAsync().Wait();
                }
                else if (args[0] == "test")
                {
                    CdsAuthenticator auth = new CdsAuthenticator();
                    auth.Username = "";
                    auth.Password = "";
                    auth.Resource = "";
                    auth.ClientId = Guid.Parse("51f81489-12ee-4a9e-aaae-a2591f45987d");
                    auth.GetAccessTokenAsync().Wait();

                    CdsService service = new CdsService(auth.Resource, auth.AccessToken);

                    while (true)
                    {
                        Console.Write("Getting ID's... ");
                        CdsReadOperation op = new CdsReadOperation();
                        op.TableIdentifier = "contacts";
                        op.AddColumn("contactid");
                        op.Top = 100;
                        JObject[] objs = service.ReadAsync(op).Result;
                        Console.WriteLine(objs.Length.ToString() + "  retrieved.");

                        //Delete each
                        foreach (JObject jo in objs)
                        {
                            JProperty? prop = jo.Property("contactid");
                            if (prop != null)
                            {
                                Guid id = Guid.Parse(prop.Value.ToString());
                                Console.Write("Deleting " + id + "... ");
                                service.DeleteRecordAsync("contacts", id.ToString()).Wait();
                                Console.WriteLine("Deleted.");
                            }
                        }

                    }

                    

                }
                else if (args[0] == "con-sql")
                {
                    PerformSqlConcurrencyTest();
                }
                else
                {
                    Console.WriteLine("I do not know that one.");
                }
            }
        }

        public static async Task PerformDataverseUploadAsync()
        {
            CdsAuthenticator auth = DataverseAuthenticator.GetCdsAuthenticator();
            await auth.GetAccessTokenAsync();
            CdsService cds = new CdsService(DataverseAuthenticator.GetCdsAuthenticator().Resource, auth.AccessToken);


            int cycles = 2;
            int ConcurrentUploads = 15;

            for (int t = 0; t < cycles; t++)
            {
                float pc = Convert.ToSingle(t) / Convert.ToSingle(cycles);

                //Check if we need to refresh the access token
                TimeSpan TimeRemaining = auth.AccessTokenExpiresUtc - DateTime.UtcNow;
                if (TimeRemaining.TotalMinutes < 3)
                {
                    Console.Write("Refreshing access token... ");
                    await auth.GetAccessTokenAsync();
                    cds = new CdsService(DataverseAuthenticator.GetCdsAuthenticator().Resource, auth.AccessToken);
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

        public static async Task PerformSharepointUploadAsync()
        {
            MicrosoftGraphHelper? mgh = null;

            //Get the mgh
            string? mgh_path = ConfigurationManager.AppSettings.Get("mgh");
            if (mgh_path != null)
            {
                if (mgh_path != "")
                {
                    string content = System.IO.File.ReadAllText(mgh_path);
                    if (content.Length > 0)
                    {
                        mgh = JsonConvert.DeserializeObject<MicrosoftGraphHelper>(content);
                    }
                }
            }

            //If MGH was null, make one
            if (mgh == null)
            {
                mgh = new MicrosoftGraphHelper();
                mgh.TenantId = Guid.Parse("1e85f23f-c0af-4bce-bb96-92014d3c1359");
                mgh.ClientId = Guid.Parse("d9571adf-0c99-4285-bd6c-85d1ad9df015");
                mgh.RedirectUrl = "https://www.google.com/";
                mgh.Scope.Add("Sites.ReadWrite.All");
                string url = mgh.AssembleAuthorizationUrl(true, true);

                Console.WriteLine("Please authenticate at the URL below:");
                Console.WriteLine(url);

                Console.WriteLine();
                Console.WriteLine("What is the code you received back?");
                Console.Write(">");
                string? code = Console.ReadLine();
                if (code != null)
                {
                    Console.Write("Getting access tokens... ");
                    await mgh.GetAccessTokenAsync(code);
                    Console.WriteLine("Got it!");
                    

                    

                }
                
            }

            //If the path is there, save it
            if (mgh_path != null)
            {
                if (mgh_path != "")
                {
                    Console.Write("Saving mgh... ");
                    System.IO.File.WriteAllText(mgh_path, JsonConvert.SerializeObject(mgh));
                    Console.WriteLine("Saved!");
                }
            }


            //To get the sites/lists if needed
            //SharepointSite[] sites = await mgh.SearchSharepointSitesAsync("");
            //Console.WriteLine(JArray.Parse(JsonConvert.SerializeObject(sites)).ToString());

            //SharepointList[] lists = await mgh.ListSharepointListsAsync(Guid.Parse("2e069086-c6f2-4735-a728-eb33b8347842"));
            //Console.WriteLine(JArray.Parse(JsonConvert.SerializeObject(lists)).ToString());
            //Console.ReadLine();

            //Get the delay time per each batch upload
            string? DelayUploadTimeStr = ConfigurationManager.AppSettings.Get("sp_delay");
            if (DelayUploadTimeStr == null)
            {
                Console.WriteLine("sp_delay was not available in the config file.");
                return;
            }
            if (DelayUploadTimeStr == "")
            {
                Console.WriteLine("sp_delay was blank in the config file.");
                return;
            }
            int DelayUploadTime = Convert.ToInt32(DelayUploadTimeStr);


            //Get the ID's
            string? spsiteidstr = ConfigurationManager.AppSettings.Get("sp_siteid");
            string? splistidstr = ConfigurationManager.AppSettings.Get("sp_listid");
            if (spsiteidstr != null && splistidstr != null)
            {
                if (spsiteidstr != "" && splistidstr != "")
                {

                    //Parse
                    Console.Write("Parsing Site ID and List ID... ");
                    Guid site_id = Guid.Parse(spsiteidstr);
                    Guid list_id = Guid.Parse(splistidstr);
                    Console.WriteLine("Parsed!");


                    for (int t = 0; t < 145145; t++) //# of batches to do
                    {

                        //Update if we have to
                        if (mgh.AccessTokenHasExpired())
                        {
                            Console.Write("Refreshing access token... ");
                            await mgh.RefreshAccessTokenAsync();
                            Console.WriteLine("Refreshed!");

                            //Save to file
                            if (mgh_path != null)
                            {
                                if (mgh_path != "")
                                {
                                    Console.Write("Saving mgh to file... ");
                                    System.IO.File.WriteAllText(mgh_path, JsonConvert.SerializeObject(mgh));
                                    Console.WriteLine("Saved!");
                                }
                            }
                        }

                        //Create the contacts
                        Contact[] ContactsToUpload = RandomContacts(40);

                        //Create the tasks
                        List<Task> ToDo = new List<Task>();
                        foreach (Contact c in ContactsToUpload)
                        {
                            JObject jo = JObject.Parse(JsonConvert.SerializeObject(c));
                            Task tsk = mgh.CreateItemAsync(site_id, list_id, jo);
                            ToDo.Add(tsk);
                        }

                        //Wait
                        Console.Write("Uploading batch # " + t.ToString("#,##0") + "... ");
                        try
                        {
                            Task.WaitAll(ToDo.ToArray());
                            Console.WriteLine("Successful!");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("THAT FAILED! Msg: " + ex.Message);
                        }
                        Console.Write("Delaying... ");
                        await Task.Delay(DelayUploadTime);
                        Console.WriteLine("Moving on!");
                    }
                }
                else
                {
                    Console.WriteLine("The Sharepoint site ID or list ID was blank in the config.");
                }
            }
            else
            {
                Console.WriteLine("The Sharepoint site ID or list ID was not in the config.");
            }
            

            
        }


        private static Contact[] RandomContacts(int count)
        {
            //Get the key values
            string? fnpath = ConfigurationManager.AppSettings.Get("rd_firstnames");
            string? lnpath = ConfigurationManager.AppSettings.Get("rd_lastnames");
            string? citiespath = ConfigurationManager.AppSettings.Get("rd_cities");

            if (fnpath == null || lnpath == null || citiespath == null)
            {
                throw new Exception("Paths to random data were not in the config file.");
            }

            if (fnpath == "" || lnpath == "" || citiespath == "")
            {
                throw new Exception("Paths to random data were BLANK in the config file.");
            }

            string[] FirstNames = System.IO.File.ReadAllText(fnpath).Split(Environment.NewLine);
            string[] LastNames = System.IO.File.ReadAllText(lnpath).Split(Environment.NewLine);
            string[] cities = System.IO.File.ReadAllText(citiespath).Split(Environment.NewLine);

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



        #region "Concurrency testing"

        public static void PerformSqlConcurrencyTest()
        {
            Dictionary<int, TimeSpan> dict = new Dictionary<int, TimeSpan>();
            List<Task> tsks = new List<Task>();

            tsks.Add(SqlConcurrencyTest(dict, 1, "select top 5 * from Contact where FirstName ='Floyd' and LastName = 'Semble'"));
            tsks.Add(SqlConcurrencyTest(dict, 2, "select top 5 * from Contact where FirstName ='Skippy' and LastName = 'Mushett'"));
            tsks.Add(SqlConcurrencyTest(dict, 3, "select top 5 * from Contact where FirstName ='Lorry' and LastName = 'Cuffin'"));
            tsks.Add(SqlConcurrencyTest(dict, 4, "select top 5 * from Contact where FirstName ='Wendall' and LastName = 'Mill'"));
            tsks.Add(SqlConcurrencyTest(dict, 5, "select top 5 * from Contact where FirstName ='Aggi' and LastName = 'Oiller'"));
            tsks.Add(SqlConcurrencyTest(dict, 6, "select top 5 * from Contact where FirstName ='Caril' and LastName = 'Flaune'"));
            tsks.Add(SqlConcurrencyTest(dict, 7, "select top 5 * from Contact where FirstName ='Winfred' and LastName = 'Aykroyd'"));
            tsks.Add(SqlConcurrencyTest(dict, 8, "select top 5 * from Contact where FirstName ='Tera' and LastName = 'Tirone'"));
            tsks.Add(SqlConcurrencyTest(dict, 9, "select top 5 * from Contact where FirstName ='Catherin' and LastName = 'Pordal'"));
            tsks.Add(SqlConcurrencyTest(dict, 10, "select top 5 * from Contact where FirstName ='Beck' and LastName = 'Sketh'"));

            tsks.Add(SqlConcurrencyTest(dict, 11, "select Id from Contact where convert(varchar, MobilePhone) like '941%'"));
            tsks.Add(SqlConcurrencyTest(dict, 12, "select Id from Contact where convert(varchar, MobilePhone) like '310%'"));
            tsks.Add(SqlConcurrencyTest(dict, 13, "select Id from Contact where convert(varchar, MobilePhone) like '212%'"));
            tsks.Add(SqlConcurrencyTest(dict, 14, "select Id from Contact where convert(varchar, MobilePhone) like '305%'"));
            tsks.Add(SqlConcurrencyTest(dict, 15, "select Id from Contact where convert(varchar, MobilePhone) like '702%'"));
            tsks.Add(SqlConcurrencyTest(dict, 16, "select Id from Contact where convert(varchar, MobilePhone) like '202%'"));
            tsks.Add(SqlConcurrencyTest(dict, 17, "select Id from Contact where convert(varchar, MobilePhone) like '415%'"));
            tsks.Add(SqlConcurrencyTest(dict, 18, "select Id from Contact where convert(varchar, MobilePhone) like '404%'"));
            tsks.Add(SqlConcurrencyTest(dict, 19, "select Id from Contact where convert(varchar, MobilePhone) like '312%'"));
            tsks.Add(SqlConcurrencyTest(dict, 20, "select Id from Contact where convert(varchar, MobilePhone) like '713%'"));
            
            tsks.Add(SqlConcurrencyTest(dict, 21, "select * from Contact where BirthDate = '19940530'"));
            tsks.Add(SqlConcurrencyTest(dict, 22, "select * from Contact where BirthDate = '19810822'"));
            tsks.Add(SqlConcurrencyTest(dict, 23, "select * from Contact where BirthDate = '19790103'"));
            tsks.Add(SqlConcurrencyTest(dict, 24, "select * from Contact where BirthDate = '19660401'"));
            tsks.Add(SqlConcurrencyTest(dict, 25, "select * from Contact where BirthDate = '20011204'"));
            tsks.Add(SqlConcurrencyTest(dict, 26, "select * from Contact where BirthDate = '19980219'"));
            tsks.Add(SqlConcurrencyTest(dict, 27, "select * from Contact where BirthDate = '19951214'"));
            tsks.Add(SqlConcurrencyTest(dict, 28, "select * from Contact where BirthDate = '20040304'"));
            tsks.Add(SqlConcurrencyTest(dict, 29, "select * from Contact where BirthDate = '19820613'"));
            tsks.Add(SqlConcurrencyTest(dict, 30, "select * from Contact where BirthDate = '19890505'"));

            tsks.Add(SqlConcurrencyTest(dict, 31, "select top 15 * from Contact where LastName = 'Semble' and BirthDate > '19910421' and AnnualIncome > 31000 order by BirthDate desc"));
            tsks.Add(SqlConcurrencyTest(dict, 32, "select top 15 * from Contact where LastName = 'Cuffin' and BirthDate > '19840209' and AnnualIncome > 43500 order by BirthDate desc"));
            tsks.Add(SqlConcurrencyTest(dict, 33, "select top 15 * from Contact where LastName = 'Mill' and BirthDate > '19621207' and AnnualIncome > 57800 order by BirthDate desc"));
            tsks.Add(SqlConcurrencyTest(dict, 34, "select top 15 * from Contact where LastName = 'Bromby' and BirthDate > '19990121' and AnnualIncome > 22000 order by BirthDate desc"));
            tsks.Add(SqlConcurrencyTest(dict, 35, "select top 15 * from Contact where LastName = 'Scartifield' and BirthDate > '20030509' and AnnualIncome > 21060 order by BirthDate desc"));
            tsks.Add(SqlConcurrencyTest(dict, 36, "select top 15 * from Contact where LastName = 'Cawthorne' and BirthDate > '19790530' and AnnualIncome > 60000 order by BirthDate desc"));
            tsks.Add(SqlConcurrencyTest(dict, 37, "select top 15 * from Contact where LastName = 'Scown' and BirthDate > '19960706' and AnnualIncome > 81900 order by BirthDate desc"));
            tsks.Add(SqlConcurrencyTest(dict, 38, "select top 15 * from Contact where LastName = 'Venner' and BirthDate > '19730923' and AnnualIncome > 79900 order by BirthDate desc"));
            tsks.Add(SqlConcurrencyTest(dict, 39, "select top 15 * from Contact where LastName = 'Benzi' and BirthDate > '19740825' and AnnualIncome > 71400 order by BirthDate desc"));
            tsks.Add(SqlConcurrencyTest(dict, 40, "select top 15 * from Contact where LastName = 'Mulvin' and BirthDate > '19780904' and AnnualIncome > 21450 order by BirthDate desc"));

            //RUN!
            Console.Write("Running tasks... ");
            DateTime dt1 = new DateTime();
            Task.WaitAll(tsks.ToArray());
            DateTime dt2 = new DateTime();
            TimeSpan ts = dt2 - dt1;
            Console.WriteLine("Complete!");
            Console.WriteLine("Completed in " + ts.TotalSeconds.ToString("#,##0") + " seconds");
            
            //Results
            Console.WriteLine("Results:");
            foreach (KeyValuePair<int, TimeSpan> kvp in dict)
            {
                Console.Write("Test # " + kvp.Key.ToString() + ": " + kvp.Value.TotalSeconds.ToString("#,##0.0"));
            }

        }

        public static async Task SqlConcurrencyTest(Dictionary<int, TimeSpan> dict, int id, string command)
        {
            DateTime dt1 = DateTime.UtcNow;
            SqlConnection sqlcon = new SqlConnection(SqlCredentialsProvider.GetSqlConnectionString());
            await sqlcon.OpenAsync();
            SqlCommand sqlcmd = new SqlCommand(command, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();
            string json = TimHanewich.Sql.SqlToolkit.ReadSqlToJson(dr);
            await sqlcon.CloseAsync();
            DateTime dt2 = DateTime.UtcNow;
            TimeSpan ts = dt2 - dt1;
            dict.Add(id, ts);
        }


        #endregion


    }
}