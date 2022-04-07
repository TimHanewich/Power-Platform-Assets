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
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using TimHanewich.Toolkit;

namespace CoreLibrary
{
    public class Program
    {
        static void Main(string[] args)
        {

            //FULL DEPLOYMENT!!!
            // DeleteAllAsync().Wait(); //Delete all (clear)
            // DeployAsync().Wait(); //Upload all data
            // TrainAsync().Wait(); //Train

            if (args.Length == 0) //For testing
            {
                CdsService service = FaceAuthenticator.AuthenticateCDSAsync().Result;
                CreateDetectedRiskPhraseAsync(service, Guid.Parse("81ef5557-d043-4ddb-8fe5-4d818398f6bc"), "FRANKY!", 0.99f).Wait();
            }
            else
            {
                if (args[0] == "scene")
                {
                    scene().Wait();
                }
                else if (args[0] == "map")
                {
                    CdsService service = FaceAuthenticator.AuthenticateCDSAsync().Result;
                    JObject map = PrepareMapAsync(service, null).Result;
                    Console.WriteLine(map.ToString());
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
            Console.WriteLine("4 - Two residents discussing getting drugs into facility.");
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

                //Authenticate CDS
                Console.Write("Authenticating CDS... ");
                CdsService service = await FaceAuthenticator.AuthenticateCDSAsync();
                ConsoleVisualsToolkit.WriteLine("Success", ConsoleColor.Green);



                ////////////////////// SETTINGS ////////////////////

                //ID's of the two fighters that will enter the cell and fight
                //They will be entering cell 1
                //They should have rivaling gangs registered in the system
                Guid fighter1 = Guid.Parse("8de5ad15-17a6-ec11-983f-0022480b18d9");
                Guid fighter2 = Guid.Parse("5a83a8c4-18a6-ec11-983f-0022480b18d9");

                //Id's of locaitons
                Guid podId = Guid.Parse("e905ec26-b5a4-ec11-983f-0022480b18d9"); //Id of the common area pod both fighters will start in
                Guid cellId = Guid.Parse("e169257c-f9a5-ec11-983f-0022480b18d9"); //Id of the cell both fighters will step into and start fighting. This is cell C1.

                ////////////////////////////////////////////////////

                //Part 1: they are in the common area
                Console.WriteLine("Part 1: They are both in the common area bickering.");
                Console.Write("Logging fighter 1 in pod... ");
                await CreateLocationDetectionAsync(service, fighter1, podId, null, null, 0.843f);
                ConsoleVisualsToolkit.WriteLine("Succuss", ConsoleColor.Green);
                Console.Write("Logging fighter 2 in pod... ");
                await CreateLocationDetectionAsync(service, fighter2, podId, null, null, 0.94f);
                ConsoleVisualsToolkit.WriteLine("Success", ConsoleColor.Green);

                //Wait
                Console.WriteLine();
                Console.Write("Press enter to proceed to next scene: Both fighters step into cell");
                Console.ReadLine();
                Console.WriteLine();

                //Part 2: they both step into the cell
                Console.WriteLine("Part 2: They both step into a cell");
                Console.Write("Logging fighter 1 in cell... ");
                await CreateLocationDetectionAsync(service, fighter1, null, cellId, null, 0.94f);
                ConsoleVisualsToolkit.WriteLine("Success", ConsoleColor.Green);
                Console.Write("Logging fighter 2 in cell... ");
                await CreateLocationDetectionAsync(service, fighter2, null, cellId, null, 0.98f);
                ConsoleVisualsToolkit.WriteLine("Success", ConsoleColor.Green);

                //Some wait time before the alert is triggered
                Console.WriteLine();
                ConsoleVisualsToolkit.Write("Press enter when you are ready to move onto the next part of this scene: an alert is triggered.", ConsoleColor.Blue);
                Console.ReadLine();
                Console.WriteLine();

                //Log the alert
                Console.Write("Logging alert in the cell... ");
                await CreateAlertAsync(service, "2 Rivaling Gang Members in Same Cell", "A high-risk situation was detected: a member of the BLACK GUERILLA FAMILY is alone with a member of the GANGSTER DISCIPLINES in Pod 1A, Cell 1.", null, cellId, null);
                ConsoleVisualsToolkit.WriteLine("Success", ConsoleColor.Green);

            } 
            else if (sceneID == "4")
            {
                //////////// SETTINGS ////////////

                Guid CellId = Guid.Parse("e8c07088-f9a5-ec11-983f-0022480b18d9"); //Cell 5
                TimeSpan TimeInBetweenEachConversationPartUpload = new TimeSpan(0, 0, 4);

                //////////////////////////////////

                Console.WriteLine("You selected scene 4: Two residents are sitting inside a cell and discussing drugs.");

                //Authenticate CDS
                Console.Write("Authenticating CDS... ");
                CdsService service = await FaceAuthenticator.AuthenticateCDSAsync();
                ConsoleVisualsToolkit.WriteLine("Success", ConsoleColor.Green);


                //First step: ensure the two participants are in the cell.



                //Second step: Start logging a conversation history
                List<string> ConversationParts = new List<string>();
                ConversationParts.Add("you can't be having the door open this morning");
                ConversationParts.Add("what does it matter");
                ConversationParts.Add("gonna come in here try to wake me up");
                ConversationParts.Add("try to get me to smoke this <b><font color=red>blunt</font></b> with them");
                ConversationParts.Add("how are they getting a <b><font color=red>blunt</font></b>");
                ConversationParts.Add("trust me");
                ConversationParts.Add("how do they light it");
                ConversationParts.Add("they get a <b><font color=red>lighter</font></b>");
                ConversationParts.Add("so they just <b><font color=red>sneak</font></b> that stuff in. and they have <b><font color=red>lighters</font></b> in here.");
                ConversationParts.Add("you know the dude with the dreads");
                ConversationParts.Add("yeah");
                ConversationParts.Add("the one who brings all the stuff in");
                ConversationParts.Add("that's crazy");
                
                
                //Wait until user indicates they are ready to move on
                Console.WriteLine("Next step is for the speech to be logged.");
                Console.Write("Press enter when you want to log the speech.");
                Console.ReadLine();

                //Log a new detected speech
                string FULL_TRANSCRIPT = "yeah i do";
                Console.Write("Logging a new detected speech... ");
                Guid ThisDetectedSpeech = await CreateSpeechDetectionAsync(service, DateTime.UtcNow, null, CellId, null, FULL_TRANSCRIPT);
                ConsoleVisualsToolkit.WriteLine("Success (" + ThisDetectedSpeech.ToString() + ")", ConsoleColor.Green);

                //Wait until user indicates they are ready to move on
                Console.WriteLine("The log was created. Next step is to APPEND EACH REMARK to that log.");
                Console.Write("Press enter to move on");
                Console.ReadLine();

                //Log each
                foreach (string s in ConversationParts)
                {
                    Console.Write("Waiting to log '");
                    ConsoleVisualsToolkit.Write(s, ConsoleColor.Cyan);
                    Console.Write("'... ");
                    await Task.Delay(TimeInBetweenEachConversationPartUpload);
                    Console.Write("Logging now... ");

                    //Add it to the full
                    FULL_TRANSCRIPT = FULL_TRANSCRIPT + "<p>" + s + "</p>";

                    //Update it
                    await UpdateSpeechDetectionTranscriptAsync(service, ThisDetectedSpeech, FULL_TRANSCRIPT);

                    //SUCCESS!
                    ConsoleVisualsToolkit.WriteLine("Success", ConsoleColor.Green);
                }
                


            }
            else if (sceneID == "4r")
            {
                //// SETTINGS //////
                string PathToAudioFile = @"C:\Users\tahan\Downloads\Face API\Scenes\Discussing sneaking in drugs\drugs.wav"; //Path to the audio file with the drug 
                ////////////////////

                Console.WriteLine("You selected scene 4: Two residents discussing getting drugs into the facility.");
                

                SpeechConfig sc = SpeechConfig.FromSubscription(FaceAuthenticator.AzureSpeechServicesKey, FaceAuthenticator.AzureSpeehServicesRegion);
                sc.SpeechRecognitionLanguage = "en-US";
                
                AudioConfig audio = AudioConfig.FromWavFileInput(PathToAudioFile);
                SpeechRecognizer sr = new SpeechRecognizer(sc, audio);

                //Set up a variable for full recognition
                string FullRecognition = null;
                FullRecognition = "Then the door opened this morning bro, what's the matter? \nThey don't come in here and try to wake me up and try to give me to smoke this blunt with them. I would get my blunt. \nTrustee. \nSleep well, they light it. \nThey get a lighter and they just think that stuff in. \nAnd they have lighters in here. \nNo no yeah. \nHe's going to bring. \nThat's crazy.";

                //Recognize all, only if it wasn't set above.
                if (FullRecognition == null)
                {
                    Console.Write("Recognizing... ");
                    HanewichTimer ht = new HanewichTimer();
                    ht.StartTimer();
                    bool KeepGoing = true;
                    while (KeepGoing)
                    {
                        SpeechRecognitionResult result = await sr.RecognizeOnceAsync();
                        if (result.Reason == ResultReason.RecognizedSpeech)
                        {
                            FullRecognition = FullRecognition + result.Text + Environment.NewLine;
                        }
                        else
                        {
                            KeepGoing = false;
                        }
                    }
                    FullRecognition = FullRecognition.Substring(0, FullRecognition.Length - 1); //Remove the last new line.
                    ht.StopTimer();
                    Console.WriteLine("Recognized in " + ht.GetElapsedTime().TotalSeconds.ToString("#,##0.0") + " seconds");
                }
                

                //Print it
                Console.WriteLine("Full recognition: ");
                Console.WriteLine(FullRecognition);
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

        public static async Task<JObject> GetMostRecentLocationDetectionForPersonAsync(CdsService service, Guid person, DateTime? before = null)
        {
            //Find the last location detection for a facility participant
            CdsReadOperation read = new CdsReadOperation();
            read.TableIdentifier = "doc_locationdetections";
            
            //Filter - for that particular participant
            CdsReadFilter filter = new CdsReadFilter();
            filter.ColumnName = "_doc_persondetected_value";
            filter.Operator = ComparisonOperator.Equals;
            filter.SetValue(person);
            read.AddFilter(filter);

            //Filter - before a specific time
            if (before.HasValue)
            {
                CdsReadFilter filter2 = new CdsReadFilter();
                filter2.ColumnName = "doc_detectedat";
                filter2.LogicalOperatorPrefix = LogicalOperator.And;
                filter2.Operator = ComparisonOperator.LessThan;
                filter2.SetValue(before.Value.ToString());
                read.AddFilter(filter2);
            }
            

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

        public static async Task CreateAlertAsync(CdsService service, string title, string description, Guid? pod, Guid? cell, Guid? recreational_area)
        {
            JObject jo = new JObject();

            jo.Add("doc_title", title);
            jo.Add("doc_description", description);

            //Pod?
            if (pod.HasValue)
            {
                jo.Add("doc_LocationPod@odata.bind", "doc_pods(" + pod.Value.ToString() + ")");
            }

            //Cell
            if (cell.HasValue)
            {
                jo.Add("doc_LocationCell@odata.bind", "doc_cells(" + cell.Value.ToString() + ")");
            }

            //Recreational area?
            if (recreational_area.HasValue)
            {
                jo.Add("doc_LocationRecreationalArea@odata.bind", "doc_recreationalareas(" + recreational_area.Value.ToString() + ")");
            }

            await service.CreateRecordAsync("doc_alerts", jo.ToString());
        }

        public static async Task<Guid> CreateSpeechDetectionAsync(CdsService service, DateTime detected_at, Guid? pod, Guid? cell, Guid? r_area, string transcript)
        {
            JObject jo = new JObject();
            Guid NewIdForThisRecord = Guid.NewGuid();
            jo.Add("doc_detectedspeechid", NewIdForThisRecord);
            jo.Add("doc_detectedat", DateTime.UtcNow);
            
            //Pod?
            if (pod.HasValue)
            {
                jo.Add("doc_DetectedInPod@odata.bind", "doc_pods(" + pod.Value.ToString() + ")");
            }

            //Cell
            if (cell.HasValue)
            {
                jo.Add("doc_DetectedInCell@odata.bind", "doc_cells(" + cell.Value.ToString() + ")");
            }

            //Recreational area?
            if (r_area.HasValue)
            {
                jo.Add("doc_DectedInRecreationalArea@odata.bind", "doc_recreationalareas(" + r_area.Value.ToString() + ")");
            }

            //Beginning transcript
            jo.Add("doc_transcript", transcript);

            await service.CreateRecordAsync("doc_detectedspeechs", jo.ToString());

            return NewIdForThisRecord;
        }

        public static async Task UpdateSpeechDetectionTranscriptAsync(CdsService service, Guid id, string new_transcript)
        {
            JObject jo = new JObject();
            jo.Add("doc_transcript", new_transcript);
            await service.UpdateRecordAsync("doc_detectedspeechs", id.ToString(), jo.ToString());
        }

        public static async Task CreateDetectedRiskPhraseAsync(CdsService service, Guid for_speech, string phrase, float score)
        {
            JObject jo = new JObject();
            jo.Add("doc_FoundinSpeech@odata.bind", "doc_detectedspeechs(" + for_speech.ToString() + ")");
            jo.Add("doc_phrase", phrase);
            jo.Add("doc_riskscore", score);
            await service.CreateRecordAsync("doc_detectedriskphrases", jo.ToString());
        }
      
        #endregion

        #region "For API - Map"

        public static async Task<JObject> PrepareMapAsync(CdsService service, DateTime? before = null)
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

            //Prepare a big list of last location detections
            //This table will tell you who it is and where it was
            List<JObject> AllLastLocationDetections = new List<JObject>();
            foreach (JObject participant in AllParticipants)
            {
                Guid ParticipantId = Guid.Parse(participant.Property("doc_facilityparticipantid").Value.ToString());
                //Get location detection history
                JObject LastLocationDetection = await GetMostRecentLocationDetectionForPersonAsync(service, ParticipantId, before);
                if (LastLocationDetection != null)
                {
                    AllLastLocationDetections.Add(LastLocationDetection);
                }
            }

            

            //For each cell, collect
            foreach (JObject cell in AllCells)
            {
                Guid cellId = Guid.Parse(cell.Property("doc_cellid").Value.ToString());
                string cellName = cell.Property("doc_id").Value.ToString(); 

                //Get a list of all current occupants ID's
                Guid[] OccupantIds = FindRoomOccupants(AllLastLocationDetections.ToArray(), cellId);

                ToReturn.Add(cellName, PrepareRoomOccupants(AllParticipants, OccupantIds));
            }
            
            //For each pod, collect
            foreach (JObject pod in AllPods)
            {
                Guid podId = Guid.Parse(pod.Property("doc_podid").Value.ToString());
                string podName = pod.Property("doc_name").Value.ToString();

                //Get a list of all current occupants ID's
                Guid[] OccupantIds = FindRoomOccupants(AllLastLocationDetections.ToArray(), podId);

                ToReturn.Add(podName, PrepareRoomOccupants(AllParticipants, OccupantIds));
            }

            //For each recreational area, collect
            foreach (JObject ra in AllRecreationalAreas)
            {
                Guid raId = Guid.Parse(ra.Property("doc_recreationalareaid").Value.ToString());
                string raName = ra.Property("doc_name").Value.ToString();

                //Get a list of all current occupants ID's
                Guid[] OccupantIds = FindRoomOccupants(AllLastLocationDetections.ToArray(), raId);

                ToReturn.Add(raName, PrepareRoomOccupants(AllParticipants, OccupantIds));
            }




            return ToReturn;
        }

        //This will scan through a list of ALL last location detections and return the GUID's of people that are inside a specific room (room meaning cell, pod, or recreational area)
        private static Guid[] FindRoomOccupants(JObject[] AllLastLocationDetections, Guid location_id)
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

        private static JArray PrepareRoomOccupants(JObject[] AllParticipants, Guid[] select_people)
        {
            List<JObject> ToReturn = new List<JObject>();
            foreach (JObject participant in AllParticipants)
            {
                Guid ParticipantId = Guid.Parse(participant.Property("doc_facilityparticipantid").Value.ToString());
                if (select_people.Contains(ParticipantId))
                {
                    JObject ToAdd = new JObject();
                    ToAdd.Add("Id", Guid.Parse(participant.Property("doc_facilityparticipantid").Value.ToString()));
                    ToAdd.Add("IdName", participant.Property("doc_id").Value.ToString());
                    ToAdd.Add("FirstName", participant.Property("doc_firstname").Value.ToString());
                    ToAdd.Add("LastName", participant.Property("doc_lastname").Value.ToString());
                    ToAdd.Add("ProfileUrl", FaceAuthenticator.CdsEnvironmentUrl + participant.Property("doc_profileimage_url").Value.ToString());
                    ToAdd.Add("PortraitUrl", FaceAuthenticator.CdsEnvironmentUrl + participant.Property("doc_portraitimage_url").Value.ToString());
                    ToReturn.Add(ToAdd);
                }
            }
            return JArray.Parse(JsonConvert.SerializeObject(ToReturn.ToArray()));
        }

        #endregion


    }
}
