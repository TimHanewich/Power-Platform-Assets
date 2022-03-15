using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace CoreLibrary
{
    class Program
    {
        static void Main(string[] args)
        {
            DeployAsync().Wait();
            
        }

        #region "Deployment - creating the necessary resources in the face API"

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
            PersonGroup pg2 = new PersonGroup();
            pg2.PersonGroupId = "residents";
            pg2.Name = "Residents";
            pg2.RecognitionModel = "recognition_04";
            pg2.UserData = "The offenders that live in the facility.";
            await client.PersonGroup.CreateAsync(pg2.PersonGroupId, pg2.Name, pg2.UserData, pg2.RecognitionModel);


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
                    PersistedFace pf = await client.PersonGroupPerson.AddFaceFromStreamAsync("residents", p.PersonId, s);
                    Console.WriteLine("Face added with ID " + pf.PersistedFaceId.ToString());
                }
            }

        }

        #endregion

    }
}
