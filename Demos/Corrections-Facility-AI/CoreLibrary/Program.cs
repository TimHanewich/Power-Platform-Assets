using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using ConsoleVisuals;
using Newtonsoft.Json;

namespace CoreLibrary
{
    class Program
    {
        static void Main(string[] args)
        {
            //DeleteAllAsync().Wait();
            //DeployAsync().Wait();
            
            //TrainAsync().Wait();

            Stream s = System.IO.File.OpenRead(@"C:\Users\tahan\Downloads\test.png");
            IdentifyResult[] results = IdentifyAsync(s).Result;
            foreach (IdentifyResult result in results)
            {
                foreach (IdentifyCandidate candidate in result.Candidates)
                {
                    Person p = GetPersonAsync(candidate.PersonId).Result;
                    Console.WriteLine(p.Name + " at " + candidate.Confidence.ToString("#,##0.00%"));
                }
            }
            
        }

        #region "Deployment - creating the necessary resources in the face API"

        public static async Task DeleteAllAsync()
        {
            IFaceClient client = FaceAuthenticator.Authenticate();
            Console.Write("Getting all PersonGroups... ");
            IList<PersonGroup> PersonGroups = await client.PersonGroup.ListAsync();
            Console.WriteLine(PersonGroups.Count.ToString("#,##0") + " person group found.");
            foreach (PersonGroup pg in PersonGroups)
            {
                Console.Write("Deleting PersonGroup '" + pg.PersonGroupId + "'... ");
                await client.PersonGroup.DeleteAsync(pg.PersonGroupId);
                Console.WriteLine("Deleted!");
            }
        }

        public static async Task DeployAsync()
        {
            //Settings
            string PersistentFaceDirectory = @"C:\Users\tahan\Downloads\Face API\Training Headshots"; //a directory with sub directories containing each persistent faces



            IFaceClient client = FaceAuthenticator.Authenticate();

            //Create the "officers" person group
            // PersonGroup pg = new PersonGroup();
            // pg.PersonGroupId = "officers";
            // pg.Name = "Officers";
            // pg.RecognitionModel = "recognition_04";
            // pg.UserData = "Officers thats work in the facility.";
            // await client.PersonGroup.CreateAsync(pg.PersonGroupId, pg.Name, pg.UserData, pg.RecognitionModel);

            //Create the "residents" person group
            Console.Write("Creating group 'residents'... ");
            PersonGroup pg2 = new PersonGroup();
            pg2.PersonGroupId = "residents";
            pg2.Name = "Residents";
            pg2.RecognitionModel = "recognition_04";
            pg2.UserData = "The offenders that live in the facility.";
            await client.PersonGroup.CreateAsync(pg2.PersonGroupId, pg2.Name, pg2.UserData, pg2.RecognitionModel);
            Console.WriteLine("Created!");


            //Create people
            foreach (string dirPath in System.IO.Directory.GetDirectories(PersistentFaceDirectory))
            {
                string Name = System.IO.Path.GetDirectoryName(dirPath);
                Console.Write("Creating person '" + Name + "'");
                Person p = await client.PersonGroupPerson.CreateAsync("residents", Name, "Automatically added.");
                Console.WriteLine("Person added! ID: " + p.PersonId.ToString());

                //Get files (images)
                foreach (string iPath in System.IO.Directory.GetFiles(dirPath))
                {
                    Console.Write("Adding '" + iPath + "'... ");
                    Stream s = System.IO.File.OpenRead(iPath);
                    try
                    {
                        PersistedFace pf = await client.PersonGroupPerson.AddFaceFromStreamAsync("residents", p.PersonId, s);
                        ConsoleVisualsToolkit.WriteLine("Face added with ID " + pf.PersistedFaceId.ToString(), ConsoleColor.Green);
                    }
                    catch
                    {
                        ConsoleVisualsToolkit.WriteLine("FAILED!", ConsoleColor.Red);
                    }
                    
                }
            }

        }

        #endregion

        #region "Training"

        public static async Task TrainAsync()
        {
            IFaceClient client = FaceAuthenticator.Authenticate();
            await client.PersonGroup.TrainAsync("residents");
            Console.WriteLine("Training initiated! You may want to give it some time though?");
        }

        #endregion

        #region "Identification"

        public static async Task<IdentifyResult[]> IdentifyAsync(Stream s)
        {
            IFaceClient client = FaceAuthenticator.Authenticate();
            IList<DetectedFace> DetectedFaces = await client.Face.DetectWithStreamAsync(s, true, false, null, "recognition_04", false, null);

            //Assemble a list of detected faces to pass to the identification endpoint
            List<Guid> ToIdentifyDetection = new List<Guid>();
            foreach (DetectedFace df in DetectedFaces)
            {
                ToIdentifyDetection.Add(df.FaceId.Value);
            }
            
            //Identify each
            IList<IdentifyResult> IdentifyResults = await client.Face.IdentifyAsync(ToIdentifyDetection, "residents");

            //Prepare and return
            List<IdentifyResult> ToReturn = new List<IdentifyResult>();
            foreach (IdentifyResult res in IdentifyResults)
            {
                ToReturn.Add(res);
            }
            return ToReturn.ToArray();
        }

        public static async Task<Person> GetPersonAsync(Guid id)
        {
            IFaceClient client = FaceAuthenticator.Authenticate();
            Person pgp = await client.PersonGroupPerson.GetAsync("residents", id);
            return pgp;
        }

        #endregion


    }
}
