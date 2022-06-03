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

            SqlConnection sqlcon = new SqlConnection(SqlCredentialsProvider.GetSqlConnectionString());
            sqlcon.Open();

            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 1, "select top 5 * from Contact where FirstName ='Floyd' and LastName = 'Semble'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 2, "select top 5 * from Contact where FirstName ='Skippy' and LastName = 'Mushett'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 3, "select top 5 * from Contact where FirstName ='Lorry' and LastName = 'Cuffin'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 4, "select top 5 * from Contact where FirstName ='Wendall' and LastName = 'Mill'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 5, "select top 5 * from Contact where FirstName ='Aggi' and LastName = 'Oiller'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 6, "select top 5 * from Contact where FirstName ='Caril' and LastName = 'Flaune'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 7, "select top 5 * from Contact where FirstName ='Winfred' and LastName = 'Aykroyd'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 8, "select top 5 * from Contact where FirstName ='Tera' and LastName = 'Tirone'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 9, "select top 5 * from Contact where FirstName ='Catherin' and LastName = 'Pordal'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 10, "select top 5 * from Contact where FirstName ='Beck' and LastName = 'Sketh'"));

            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 11, "select Id from Contact where convert(varchar, MobilePhone) like '941%'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 12, "select Id from Contact where convert(varchar, MobilePhone) like '310%'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 13, "select Id from Contact where convert(varchar, MobilePhone) like '212%'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 14, "select Id from Contact where convert(varchar, MobilePhone) like '305%'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 15, "select Id from Contact where convert(varchar, MobilePhone) like '702%'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 16, "select Id from Contact where convert(varchar, MobilePhone) like '202%'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 17, "select Id from Contact where convert(varchar, MobilePhone) like '415%'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 18, "select Id from Contact where convert(varchar, MobilePhone) like '404%'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 19, "select Id from Contact where convert(varchar, MobilePhone) like '312%'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 20, "select Id from Contact where convert(varchar, MobilePhone) like '713%'"));
            
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 21, "select * from Contact where BirthDate = '19940530'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 22, "select * from Contact where BirthDate = '19810822'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 23, "select * from Contact where BirthDate = '19790103'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 24, "select * from Contact where BirthDate = '19660401'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 25, "select * from Contact where BirthDate = '20011204'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 26, "select * from Contact where BirthDate = '19980219'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 27, "select * from Contact where BirthDate = '19951214'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 28, "select * from Contact where BirthDate = '20040304'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 29, "select * from Contact where BirthDate = '19820613'"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 30, "select * from Contact where BirthDate = '19890505'"));

            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 31, "select top 15 * from Contact where LastName = 'Semble' and BirthDate > '19910421' and AnnualIncome > 31000 order by BirthDate desc"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 32, "select top 15 * from Contact where LastName = 'Cuffin' and BirthDate > '19840209' and AnnualIncome > 43500 order by BirthDate desc"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 33, "select top 15 * from Contact where LastName = 'Mill' and BirthDate > '19621207' and AnnualIncome > 57800 order by BirthDate desc"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 34, "select top 15 * from Contact where LastName = 'Bromby' and BirthDate > '19990121' and AnnualIncome > 22000 order by BirthDate desc"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 35, "select top 15 * from Contact where LastName = 'Scartifield' and BirthDate > '20030509' and AnnualIncome > 21060 order by BirthDate desc"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 36, "select top 15 * from Contact where LastName = 'Cawthorne' and BirthDate > '19790530' and AnnualIncome > 60000 order by BirthDate desc"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 37, "select top 15 * from Contact where LastName = 'Scown' and BirthDate > '19960706' and AnnualIncome > 81900 order by BirthDate desc"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 38, "select top 15 * from Contact where LastName = 'Venner' and BirthDate > '19730923' and AnnualIncome > 79900 order by BirthDate desc"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 39, "select top 15 * from Contact where LastName = 'Benzi' and BirthDate > '19740825' and AnnualIncome > 71400 order by BirthDate desc"));
            tsks.Add(SqlConcurrencyTest(dict, sqlcon, 40, "select top 15 * from Contact where LastName = 'Mulvin' and BirthDate > '19780904' and AnnualIncome > 21450 order by BirthDate desc"));

            //RUN!
            Console.WriteLine("Running tasks... ");
            DateTime dt1 = DateTime.UtcNow;
            Task.WaitAll(tsks.ToArray());
            DateTime dt2 = DateTime.UtcNow;
            TimeSpan ts = dt2 - dt1;
            Console.WriteLine("Complete!");
            Console.WriteLine("Completed in " + ts.TotalSeconds.ToString("#,##0") + " seconds");
            
            //Results
            Console.WriteLine("Results:");
            foreach (KeyValuePair<int, TimeSpan> kvp in dict)
            {
                Console.WriteLine("Test # " + kvp.Key.ToString() + ": " + kvp.Value.TotalSeconds.ToString("#,##0.0"));
            }

        }

        public static async Task SqlConcurrencyTest(Dictionary<int, TimeSpan> dict, SqlConnection sqlcon, int id, string command)
        {
            DateTime dt1 = DateTime.UtcNow;
            
            SqlCommand sqlcmd = new SqlCommand(command, sqlcon);
            sqlcmd.CommandTimeout = 5000000;
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();
            string json = TimHanewich.Sql.SqlToolkit.ReadSqlToJson(dr);
            //await sqlcon.CloseAsync();
            DateTime dt2 = DateTime.UtcNow;
            TimeSpan ts = dt2 - dt1;
            dict.Add(id, ts);
            Console.WriteLine("Test # " + id.ToString() + " completed in " + ts.TotalSeconds.ToString("#,##0.0") + " seconds!");
        }

        public static void PerformCdsConcurrencyTest()
        {
            CdsAuthenticator auth = DataverseAuthenticator.GetCdsAuthenticator();
            auth.GetAccessTokenAsync().Wait();
            string token = auth.AccessToken;

            Dictionary<int, TimeSpan> dict = new Dictionary<int, TimeSpan>();
            List<Task> tsks = new List<Task>();

            tsks.Add(CdsConcurrencyTest(dict, 1, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=firstname eq 'Floyd' and lastname eq 'Semble'"));
            tsks.Add(CdsConcurrencyTest(dict, 2, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=firstname eq 'Skippy' and lastname eq 'Mushett'"));
            tsks.Add(CdsConcurrencyTest(dict, 3, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=firstname eq 'Lorry' and lastname eq 'Cuffin'"));
            tsks.Add(CdsConcurrencyTest(dict, 4, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=firstname eq 'Wendall' and lastname eq 'Mill'"));
            tsks.Add(CdsConcurrencyTest(dict, 5, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=firstname eq 'Aggi' and lastname eq 'Oiller'"));
            tsks.Add(CdsConcurrencyTest(dict, 6, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=firstname eq 'Caril' and lastname eq 'Flaune'"));
            tsks.Add(CdsConcurrencyTest(dict, 7, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=firstname eq 'Winfred' and lastname eq 'Aykroyd'"));
            tsks.Add(CdsConcurrencyTest(dict, 8, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=firstname eq 'Tera' and lastname eq 'Tirone'"));
            tsks.Add(CdsConcurrencyTest(dict, 9, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=firstname eq 'Catherin' and lastname eq 'Pordal'"));
            tsks.Add(CdsConcurrencyTest(dict, 10, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=firstname eq 'Beck' and lastname eq 'Sketh'"));

            tsks.Add(CdsConcurrencyTest(dict, 11, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=startswith(mobilephone, '941')&$top=8"));
            tsks.Add(CdsConcurrencyTest(dict, 12, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=startswith(mobilephone, '310')&$top=8"));
            tsks.Add(CdsConcurrencyTest(dict, 13, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=startswith(mobilephone, '212')&$top=8"));
            tsks.Add(CdsConcurrencyTest(dict, 14, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=startswith(mobilephone, '305')&$top=8"));
            tsks.Add(CdsConcurrencyTest(dict, 15, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=startswith(mobilephone, '702')&$top=8"));
            tsks.Add(CdsConcurrencyTest(dict, 16, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=startswith(mobilephone, '202')&$top=8"));
            tsks.Add(CdsConcurrencyTest(dict, 17, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=startswith(mobilephone, '415')&$top=8"));
            tsks.Add(CdsConcurrencyTest(dict, 18, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=startswith(mobilephone, '404')&$top=8"));
            tsks.Add(CdsConcurrencyTest(dict, 19, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=startswith(mobilephone, '312')&$top=8"));
            tsks.Add(CdsConcurrencyTest(dict, 20, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=startswith(mobilephone, '713')&$top=8"));
        
            tsks.Add(CdsConcurrencyTest(dict, 21, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=birthdate eq 1994-95-30"));
            tsks.Add(CdsConcurrencyTest(dict, 22, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=birthdate eq 1981-08-22"));
            tsks.Add(CdsConcurrencyTest(dict, 23, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=birthdate eq 1979-01-03"));
            tsks.Add(CdsConcurrencyTest(dict, 24, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=birthdate eq 1966-04-01"));
            tsks.Add(CdsConcurrencyTest(dict, 25, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=birthdate eq 2001-12-04"));
            tsks.Add(CdsConcurrencyTest(dict, 26, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=birthdate eq 1998-02-19"));
            tsks.Add(CdsConcurrencyTest(dict, 27, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=birthdate eq 1995-12-14"));
            tsks.Add(CdsConcurrencyTest(dict, 28, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=birthdate eq 2004-03-04"));
            tsks.Add(CdsConcurrencyTest(dict, 29, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=birthdate eq 1982-06-13"));
            tsks.Add(CdsConcurrencyTest(dict, 30, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=birthdate eq 1989-05-05"));

            tsks.Add(CdsConcurrencyTest(dict, 31, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$top=15&$orderby=birthdate desc&$filter=lastname eq 'Semble' and birthdate gt 1991-04-21 and annualincome gt 31000"));
            tsks.Add(CdsConcurrencyTest(dict, 32, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$top=15&$orderby=birthdate desc&$filter=lastname eq 'Cuffin' and birthdate gt 1984-02-09 and annualincome gt 43500"));
            tsks.Add(CdsConcurrencyTest(dict, 33, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$top=15&$orderby=birthdate desc&$filter=lastname eq 'Mill' and birthdate gt 1962-12-07 and annualincome gt 57800"));
            tsks.Add(CdsConcurrencyTest(dict, 34, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$top=15&$orderby=birthdate desc&$filter=lastname eq 'Bromby' and birthdate gt 1999-01-21 and annualincome gt 22000"));
            tsks.Add(CdsConcurrencyTest(dict, 35, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$top=15&$orderby=birthdate desc&$filter=lastname eq 'Scartifield' and birthdate gt 2003-05-09 and annualincome gt 21060"));
            tsks.Add(CdsConcurrencyTest(dict, 36, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$top=15&$orderby=birthdate desc&$filter=lastname eq 'Cawthorne' and birthdate gt 1979-05-30 and annualincome gt 60000"));
            tsks.Add(CdsConcurrencyTest(dict, 37, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$top=15&$orderby=birthdate desc&$filter=lastname eq 'Scown' and birthdate gt 1996-07-06 and annualincome gt 81900"));
            tsks.Add(CdsConcurrencyTest(dict, 38, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$top=15&$orderby=birthdate desc&$filter=lastname eq 'Venner' and birthdate gt 1973-09-23 and annualincome gt 79900"));
            tsks.Add(CdsConcurrencyTest(dict, 39, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$top=15&$orderby=birthdate desc&$filter=lastname eq 'Benzi' and birthdate gt 1974-08-25 and annualincome gt 71400"));
            tsks.Add(CdsConcurrencyTest(dict, 40, token, "https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$top=15&$orderby=birthdate desc&$filter=lastname eq 'Mulvin' and birthdate gt 1978-09-04 and annualincome gt 21450"));
        
            //RUN!
            Console.WriteLine("Running tasks... ");
            DateTime dt1 = DateTime.UtcNow;
            Task.WaitAll(tsks.ToArray());
            DateTime dt2 = DateTime.UtcNow;
            TimeSpan ts = dt2 - dt1;
            Console.WriteLine("Complete!");
            Console.WriteLine("Completed in " + ts.TotalSeconds.ToString("#,##0") + " seconds");
            
            //Results
            Console.WriteLine("Results:");
            foreach (KeyValuePair<int, TimeSpan> kvp in dict)
            {
                Console.WriteLine("Test # " + kvp.Key.ToString() + ": " + kvp.Value.TotalSeconds.ToString("#,##0.0"));
            }        
        }

        public static async Task CdsConcurrencyTest(Dictionary<int, TimeSpan> dict, int id, string token, string url)
        {
            HttpRequestMessage req = new HttpRequestMessage();
            req.Method = HttpMethod.Get;
            req.Headers.Add("Authorization", "Bearer " + token);
            req.RequestUri = new Uri(url);
            HttpClient hc = new HttpClient();
            DateTime dt1 = DateTime.UtcNow;
            HttpResponseMessage resp = await hc.SendAsync(req);
            if (resp.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception("Query '" + url + "' failed with code " + resp.StatusCode.ToString());
            }
            DateTime dt2 = DateTime.UtcNow;
            TimeSpan ts = dt2 - dt1;
            dict.Add(id, ts);
            Console.WriteLine("Test # " + id.ToString() + " completed in " + ts.TotalSeconds.ToString("#,##0.0") + " seconds!");
        }


        #endregion


    }
}