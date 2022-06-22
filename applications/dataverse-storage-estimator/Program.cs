using System;
using TimHanewich.Cds;
using TimHanewich.Cds.Metadata;
using System.Threading;
using System.Threading.Tasks;
using ConsoleVisuals;
using System.Collections.Generic;

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
            PrintEntityMetadataSummaries(summaries);

            //Get the table that the user is targeting
            Console.WriteLine("You can find a list of every table in the Dataverse environment listed above.");
            Console.WriteLine("Enter the ID # (left column) of the table you are targeting.");
            Console.WriteLine("Alternatively, enter in text to search the tables and get a shorter list.");
            EntityMetadataSummary[] ScopedSummaries = summaries;
            string TargetTableSchemaName = "";
            while (TargetTableSchemaName == "")
            {
                Console.Write("Table # or search: ");
                string? input = Console.ReadLine();
                if (input != null)
                {
                    //Firstly, is this a number? if it is, it is a selection
                    int? num = null;
                    try
                    {
                        num = Convert.ToInt32(input);
                    }
                    catch
                    {

                    }
                    if (num.HasValue) //they provided a number
                    {
                        try
                        {
                            TargetTableSchemaName = ScopedSummaries[num.Value - 1].SchemaName;
                        }
                        catch
                        {
                            ConsoleVisualsToolkit.WriteLine("Number '" + num.Value.ToString() + "' is not valid.", ConsoleColor.Red);
                        }
                    }
                    else //They provided a string value (to search)
                    {

                        //Search and assemble results
                        List<EntityMetadataSummary> search_ems = new List<EntityMetadataSummary>();
                        foreach (EntityMetadataSummary ems in summaries)
                        {
                            bool ShouldInclude = false;
                            if (ems.LogicalName != null)
                            {
                                if (ems.LogicalName.ToLower().Contains(input.ToLower()))
                                {
                                    ShouldInclude = true;
                                }
                            }
                            if (ems.SchemaName != null)
                            {
                                if (ems.SchemaName.ToLower().Contains(input.ToLower()))
                                {
                                    ShouldInclude = true;
                                }
                            }
                            if (ems.DisplayName != null)
                            {
                                if (ems.DisplayName.ToLower().Contains(input.ToLower()))
                                {
                                    ShouldInclude = true;
                                }
                            }
                            if (ShouldInclude)
                            {
                                search_ems.Add(ems);
                            }
                        }

                        //Print results
                        PrintEntityMetadataSummaries(search_ems.ToArray());

                        //Set the scope to the search results
                        ScopedSummaries = search_ems.ToArray();
                    }
                }
            }

            //Confirm selection
            Console.Write("You have selected '");
            ConsoleVisualsToolkit.Write(TargetTableSchemaName, ConsoleColor.Cyan);
            Console.WriteLine("' as your target table!");

            //Find the logical name for that table
            string TargetTableLogicalName = "";
            foreach (EntityMetadataSummary ems in summaries)
            {
                if (ems.SchemaName == TargetTableSchemaName)
                {
                    if (ems.LogicalName != null)
                    {
                        TargetTableLogicalName = ems.LogicalName;
                    }
                    else
                    {
                        TargetTableLogicalName = TargetTableSchemaName;
                    }
                }
            }


            //Get metadata for this table
            Console.Write("Acquiring schema for '" + TargetTableSchemaName + "'... ");
            EntityMetadata meta;
            try
            {
                meta = await cds.GetEntityMetadataAsync(TargetTableLogicalName);
                ConsoleVisualsToolkit.WriteLine("Success!", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                ConsoleVisualsToolkit.WriteLine("Failure! " + ex.Message, ConsoleColor.Red);
                return;
            }
            Console.WriteLine();


            //Calculate
            Console.WriteLine("Proceeding now to calculations...");
            int ByteCount = 0;
            foreach (AttributeMetadata am in meta.Attributes)
            {
                switch (am.AttributeType)
                {
                    case AttributeType.Other:
                        break;
                    case AttributeType.String:

                        if (am.IsCustomAttribute == true)
                        {
                            Console.Write("You have a text field, ");
                            ConsoleVisualsToolkit.Write(am.DisplayName, ConsoleColor.Cyan);
                            Console.Write(" (");
                            ConsoleVisualsToolkit.Write(am.SchemaName, ConsoleColor.Cyan);
                            Console.WriteLine(")");
                            Console.Write("How many characters, on average, do you expect this field to be occupied by? > ");
                            int? length = null;
                            while (length == null)
                            {
                                string? ip = Console.ReadLine();
                                if (ip != null)
                                {
                                    try
                                    {
                                        length = Convert.ToInt32(ip);
                                    }
                                    catch
                                    {
                                        ConsoleVisualsToolkit.WriteLine("'" + ip + "' is not a valid integer.", ConsoleColor.Red);
                                    }
                                }
                            }

                            //Add it to the byte count
                            ByteCount = ByteCount + length.Value + 2; //1 byte per character and then 2 bytes to store length information
                        }

                        break;
                    case AttributeType.Money:
                        ByteCount = ByteCount + 8; //Takes 8 bytes (bigint/double)
                        ByteCount = ByteCount + 8; //For the "base" field (read only)
                        ByteCount = ByteCount + 16; //For the lookup to the currency it belongs to (titled "_transactioncurrencyid_value")
                        ByteCount = ByteCount + 8; //For the "exchangerate" field
                        break;
                    case AttributeType.Integer:
                        ByteCount = ByteCount + 4;
                        break;
                    case AttributeType.Lookup:
                        ByteCount = ByteCount + 16; //A lookup uses a GUID, which is 16 bytes
                        break;
                    case AttributeType.Boolean:
                       ByteCount = ByteCount + 1; //It uses one bit, but is stored as one byte
                       break;
                    case AttributeType.DateTime:
                        ByteCount = ByteCount + 8;
                        break;
                    case AttributeType.Memo:
                        break;
                    case AttributeType.Decimal:
                        ByteCount = ByteCount + 16;
                        break;
                    case AttributeType.Customer:
                        ByteCount = ByteCount + 16; // A "customer" field is a lookup to a contact or account table. So 16 bytes for the lookup, but is there more storage being used to point to what table the reference points to? (polymorphism)
                        break; 
                    case AttributeType.Virtual:
                        break;
                    case AttributeType.Picklist:
                        ByteCount = ByteCount + 4; //Pick list uses integer
                        break;
                    case AttributeType.Double:
                        ByteCount = ByteCount + 8;
                        break;
                    case AttributeType.BigInt:
                        ByteCount = ByteCount + 8;
                        break;
                    case AttributeType.EntityName:
                        break;
                    case AttributeType.State:
                        ByteCount = ByteCount + 1; //Assuming 1 byte (it is 0 or 1)
                        break;
                    case AttributeType.Owner:
                        ByteCount = ByteCount + 16; //It is a lookup (GUID)
                        break;
                    case AttributeType.Uniqueidentifier: //GUID
                        ByteCount = ByteCount + 16;
                        break; 
                    case AttributeType.Status:
                        ByteCount = ByteCount + 1;
                        break;
                }
            }
        
            //Print the results
            Console.WriteLine();
            Console.WriteLine("Storage size calculated!", ConsoleColor.Green);
            Console.WriteLine();
            ConsoleTable ct = ConsoleTable.Create("Records", "Size", "Unit");
            ct.AddRow("1", ByteCount.ToString("#,##0"), "bytes");
            ct.AddRow("1,000", BytesToGigabytes(ByteCount * 1000).ToString("#,##0.000"), "Gigabyte");
            ct.AddRow("1,000,000", BytesToGigabytes(ByteCount * 1000000).ToString("#,##0.000"), "Gigabyte");
            ct.AddRow("100,000,000", BytesToGigabytes(ByteCount * 100000000).ToString("#,##0.0"), "Gigabyte");
            ct.WriteTable();
        }

        private static void PrintEntityMetadataSummaries(EntityMetadataSummary[] summaries)
        {
            ConsoleTable ct = ConsoleTable.Create("#", "Schema Name", "Display Name");
            for (int t = 0; t < summaries.Length; t++)
            {
                ct.AddRow((t+1).ToString("#,##0"), summaries[t].SchemaName, summaries[t].DisplayName);
            }
            Console.WriteLine();
            ct.WriteTable();
        }

        private static float BytesToGigabytes(int bytes)
        {
            float mb = Convert.ToSingle(bytes) / 1000000f;
            float gb = mb / 1000f;
            return gb;
        }




    }
}