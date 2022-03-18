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
using TimHanewich.Cds.Helpers;
using TimHanewich.Cds.AdvancedRead;
using TimHanewich.Cds;
using Newtonsoft.Json.Linq;

namespace CoreLibrary
{
    class Program
    {
        static void Main(string[] args)
        {

            //FULL DEPLOYMENT!!!
            // DeleteAllAsync().Wait(); //Delete all (clear)
            // DeployAsync().Wait(); //Upload all data
            // TrainAsync().Wait(); //Train


            
            CdsService service = FaceAuthenticator.AuthenticateCDSAsync().Result;
            

            
            JObject map = PrepareMapAsync(service).Result;
            Console.WriteLine(map.ToString());

            
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
                string Name = System.IO.Path.GetFileName(dirPath);
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

        #region "Printing of all people"

        public static async Task PrintAllPeopleAsync()
        {
            IFaceClient client = FaceAuthenticator.Authenticate();
            IList<Person> people = await client.PersonGroupPerson.ListAsync("residents");
            foreach (Person p in people)
            {
                Console.WriteLine(p.Name + " = " + p.PersonId.ToString());
            }
        }

        #endregion

        #region "SCENES"

        public static async Task scene()
        {


            Console.WriteLine("Script a scene:");
            Console.WriteLine();
            Console.WriteLine("1 - Matt Fellows is in his cells and then is found in the pod area.");
            Console.WriteLine("2 - Cameras on entrances detect Jaclin Owens walks from the gym --> pod --> cell.");
            Console.WriteLine("3 - A fight breaks out between two rivaling gang members in a pod.");
            Console.WriteLine();
            Console.Write("What scene do you want to play? >");
            string sceneID = Console.ReadLine();
            

            if (sceneID == "1")
            {
                Console.WriteLine("You selected scene 1.");

                //Authenticate CDS
                Console.Write("Authenticating CDS... ");
                CdsService service = await FaceAuthenticator.AuthenticateCDSAsync();
                ConsoleVisualsToolkit.WriteLine("Success", ConsoleColor.Green);

                ConsoleVisualsToolkit.WriteLine("Part 1: Matt is in his cell.");

                //Save location history that matt is in his cell
                Guid MattId = Guid.Parse("fd33b13a-68a5-ec11-983f-0022480b18d9");
                Guid MattCellId = Guid.Parse("c2c07088-f9a5-ec11-983f-0022480b18d9"); //Cell C4 (there are 12 cells in pod 1A) in El Dorado)
                Console.Write("Creating location detection for Matt in his cell... ");
                await CreateLocationDetectionAsync(service, MattId, null, MattCellId, null, 0.932f);
                ConsoleVisualsToolkit.WriteLine("Success", ConsoleColor.Green);

                //Wait for next part
                Console.WriteLine();
                Console.WriteLine("The next part of this scene is Matt being seen in the common area.");
                Console.Write("Press enter when you are ready to proceed to this scene.");
                Console.ReadLine();
                Console.WriteLine();

                //Save location history of Matt in the common area pod
                Guid MattInPodId = Guid.Parse("e905ec26-b5a4-ec11-983f-0022480b18d9"); //The ID of the pod Matt's cell is in.
                Console.Write("Creating location detection for Matt in the pod... ");
                await CreateLocationDetectionAsync(service, MattId, MattInPodId, null, null, 0.9732f);
                ConsoleVisualsToolkit.WriteLine("Success!", ConsoleColor.Green);

                Console.WriteLine();
                ConsoleVisualsToolkit.WriteLine("Scene complete!", ConsoleColor.Green);
            }
            else if (sceneID == "2")
            {
                Console.WriteLine("You have selected scene 2: Jaclin walks from gym to pod to cell.");

                //Authenticate CDS
                Console.Write("Authenticating CDS... ");
                CdsService service = await FaceAuthenticator.AuthenticateCDSAsync();
                ConsoleVisualsToolkit.WriteLine("Success", ConsoleColor.Green);

                //Vars needed for this scene
                Guid jaclinId = Guid.Parse("dd84065f-68a5-ec11-983f-0022480b18d9");
                Guid gymId = Guid.Parse("7cc626f3-0fa6-ec11-983f-0022480b18d9"); //A recreational area record
                Guid podId = Guid.Parse("e905ec26-b5a4-ec11-983f-0022480b18d9"); //pod 1A
                Guid cellId = Guid.Parse("98212c95-f9a5-ec11-983f-0022480b18d9"); //Cell C9

                //Part 1: Jaclin in basketball court
                Console.Write("Logging Jaclin in gym... ");
                await CreateLocationDetectionAsync(service, jaclinId, null, null, gymId, 0.8793f);
                ConsoleVisualsToolkit.WriteLine("Success", ConsoleColor.Green);
                
                //Wait
                Console.WriteLine();
                Console.Write("Press enter to proceed to next scene: Jaclin walks into pod.");
                Console.ReadLine();
                Console.WriteLine();

                //Part 2: Jaclin in pod
                Console.Write("Logging Jaclin in pod... ");
                await CreateLocationDetectionAsync(service, jaclinId, podId, null, null, 0.9543f);
                ConsoleVisualsToolkit.WriteLine("Success", ConsoleColor.Green);

                //Wait
                Console.WriteLine();
                Console.Write("Press enter to proceed to next scene: Jaclin walks into cell.");
                Console.ReadLine();
                Console.WriteLine();
                
                //Part 2: Jaclin in pod
                Console.Write("Logging Jaclin in cell... ");
                await CreateLocationDetectionAsync(service, jaclinId, null, cellId, null, 0.9543f);
                ConsoleVisualsToolkit.WriteLine("Success", ConsoleColor.Green);
            }
            else if (sceneID == "3")
            {
                Console.WriteLine("You selected scene 3: fight scene");

            }
            else
            {
                Console.WriteLine("Scene '" + sceneID + "' not recognized or not available yet.");
            }
        }

        #endregion

        #region "Toolkit"

        //Finds the GUID of a facility participant in CDS by their Face API ID (the Face API ID that is stored in CDS)
        public static async Task<Guid?> FindFacilityParticipantByFaceApiIdAsync(CdsService service, Guid id)
        {
            //Find the Faciliy Participant in CDS according to GUID
            CdsReadOperation read = new CdsReadOperation();
            read.TableIdentifier = "doc_facilityparticipants";
            CdsReadFilter filter = new CdsReadFilter();
            filter.ColumnName = "doc_faceid";
            filter.Operator = ComparisonOperator.Equals;
            filter.SetValue(id.ToString());
            read.AddFilter(filter);
            read.AddColumn("doc_facilityparticipantid");
            JObject[] objects = await service.ReadAsync(read);
            if (objects.Length == 0)
            {
                return null;
            }
            else
            {
                return Guid.Parse(objects[0].Property("doc_facilityparticipantid").Value.ToString());
            }
        }

        public static async Task CreateLocationDetectionAsync(CdsService service, Guid person, Guid? pod, Guid? cell, Guid? recreational_area, float confidence)
        {
            JObject jo = new JObject();
            jo.Add("doc_PersonDetected@odata.bind", "doc_facilityparticipants(" + person.ToString() + ")");

            //Pod?
            if (pod.HasValue)
            {
                jo.Add("doc_DetectedinPod@odata.bind", "doc_pods(" + pod.Value.ToString() + ")");
            }

            //Cell
            if (cell.HasValue)
            {
                jo.Add("doc_DetectedinCell@odata.bind", "doc_cells(" + cell.Value.ToString() + ")");
            }

            //Recreational area?
            if (recreational_area.HasValue)
            {
                jo.Add("doc_DetectedinRecreationalArea@odata.bind", "doc_recreationalareas(" + recreational_area.Value.ToString() + ")");
            }

            //Confidence
            jo.Add("doc_facialrecognitionconfidence", confidence);

            //Detected At
            jo.Add("doc_detectedat", DateTime.UtcNow);


            //Upload
            await service.CreateRecordAsync("doc_locationdetections", jo.ToString());
        }

        public static async Task<JObject> GetMostRecentLocationDetectionForPersonAsync(CdsService service, Guid person)
        {
            //Find the last location detection for a facility participant
            CdsReadOperation read = new CdsReadOperation();
            read.TableIdentifier = "doc_locationdetections";
            
            //Filter - for that particular participant
            CdsReadFilter filter = new CdsReadFilter();
            filter.ColumnName = "_doc_persondetected_value";
            filter.SetValue(person);
            read.AddFilter(filter);

            //Ordering (sorting) - get most recent
            CdsReadOrder order = new CdsReadOrder();
            order.ColumnName = "doc_detectedat";
            order.Direction = OrderDirection.Descending;
            read.AddOrdering(order);

            //Top - only get the top 1
            read.Top = 1;

            JObject[] records = await service.ReadAsync(read);
            if (records.Length > 0)
            {
                return records[0];
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region "For API - Preparation of 'where is everyone?' body"

        public static async Task<JObject> PrepareMapAsync(CdsService service)
        {
            //Get a list of all participants
            JObject[] AllParticipants = await service.GetRecordsAsync("doc_facilityparticipants");

            //Get a list of all cells
            JObject[] AllCells = await service.GetRecordsAsync("doc_cells");

            //Get a list of all pods
            JObject[] AllPods = await service.GetRecordsAsync("doc_pods");

            //Get a list of all recreational facilities
            JObject[] AllRecreationalAreas = await service.GetRecordsAsync("doc_recreationalareas");


            //Construct the body to return
            JObject ToReturn = new JObject();

            //Construct the variables to return. Each of these is a LIST of objects that represent people
            List<JObject> C1 = new List<JObject>();
            List<JObject> C2 = new List<JObject>();
            List<JObject> C3 = new List<JObject>();
            List<JObject> C4 = new List<JObject>();
            List<JObject> C5 = new List<JObject>();
            List<JObject> C6 = new List<JObject>();
            List<JObject> C7 = new List<JObject>();
            List<JObject> C8 = new List<JObject>();
            List<JObject> C9 = new List<JObject>();
            List<JObject> C10 = new List<JObject>();
            List<JObject> C11 = new List<JObject>();
            List<JObject> C12 = new List<JObject>();
            List<JObject> CommonArea = new List<JObject>();
            List<JObject> Gym = new List<JObject>();

            //Prepare a big list of last location detections
            //This table will tell you who it is and where it was
            List<JObject> AllLastLocationDetections = new List<JObject>();
            foreach (JObject participant in AllParticipants)
            {
                Guid ParticipantId = Guid.Parse(participant.Property("doc_facilityparticipantid").Value.ToString());
                //Get location detection history
                JObject LastLocationDetection = await GetMostRecentLocationDetectionForPersonAsync(service, ParticipantId);
                if (LastLocationDetection != null)
                {
                    AllLastLocationDetections.Add(LastLocationDetection);
                }
            }

            //For each cell, prepare 
            foreach (JObject cell in AllCells)
            {
                Guid cellId = Guid.Parse(cell.Property("doc_cellid").Value.ToString());
                string cellName = cell.Property("doc_id").Value.ToString();

                //Get a list of all current occupants ID's
                Guid[] OccupantIds = PrepareRoomOccupants(AllLastLocationDetections.ToArray(), cellId);

                ToReturn.Add(cellName, OccupantIds.Length);
            }

            //For each pod, prepare
            foreach (JObject pod in AllPods)
            {
                Guid podId = Guid.Parse(pod.Property("doc_podid").Value.ToString());
                string podName = pod.Property("doc_name").Value.ToString();

                //Get a list of all current occupants ID's
                Guid[] OccupantIds = PrepareRoomOccupants(AllLastLocationDetections.ToArray(), podId);

                ToReturn.Add(podName, OccupantIds.Length);
            }

            //For each recreational area, prepare
            foreach (JObject ra in AllRecreationalAreas)
            {
                Guid raId = Guid.Parse(ra.Property("doc_recreationalareaid").Value.ToString());
                string raName = ra.Property("doc_name").Value.ToString();

                //Get a list of all current occupants ID's
                Guid[] OccupantIds = PrepareRoomOccupants(AllLastLocationDetections.ToArray(), raId);

                ToReturn.Add(raName, OccupantIds.Length);
            }


            return ToReturn;
        }

        //This will scan through a list of ALL last location detections and return the GUID's of people that are inside a specific room (room meaning cell, pod, or recreational area)
        private static Guid[] PrepareRoomOccupants(JObject[] AllLastLocationDetections, Guid location_id)
        {
            List<Guid> ToReturn = new List<Guid>();

            foreach (JObject LastLocationDetection in AllLastLocationDetections)
            {
                //Id of the person it belongs to
                Guid DetectedPersonId = Guid.Parse(LastLocationDetection.Property("_doc_persondetected_value").Value.ToString());

                //Get the value of the location that this corresponds to
                JProperty diCell = LastLocationDetection.Property("_doc_detectedincell_value");
                JProperty diRecreationalArea = LastLocationDetection.Property("_doc_detectedinrecreationalarea_value");
                JProperty diPod = LastLocationDetection.Property("_doc_detectedinpod_value");

                //Cell?
                if (diCell.Value.Type != JTokenType.Null)
                {
                    Guid id = Guid.Parse(diCell.Value.ToString());
                    if (id == location_id)
                    {
                        ToReturn.Add(DetectedPersonId);
                    }
                }

                //Pod?
                if (diPod.Value.Type != JTokenType.Null)
                {
                    Guid id = Guid.Parse(diPod.Value.ToString());
                    if (id == location_id)
                    {
                        ToReturn.Add(DetectedPersonId);
                    }
                }

                //Recreational Area?
                if (diRecreationalArea.Value.Type != JTokenType.Null)
                {
                    Guid id = Guid.Parse(diRecreationalArea.Value.ToString());
                    if (id == location_id)
                    {
                        ToReturn.Add(DetectedPersonId);
                    }
                }

            }

            return ToReturn.ToArray();
        }

        #endregion


    }
}
