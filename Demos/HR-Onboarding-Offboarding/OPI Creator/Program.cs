using System;
using TimHanewich.Cds;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OPI_Creator
{
    class Program
    {
        static void Main(string[] args)
        {
            RunProcess();
        }

        public static void RunProcess()
        {
            string org_name = "orgf6809d85";
            string env_url = "https://" + org_name + ".crm.dynamics.com/";
            string client_id = "a5ad2463-0a21-4e98-bae8-dc36bdf432ba";
            string authurl = "https://login.microsoftonline.com/common/oauth2/authorize?resource=" + env_url + "&client_id=" + client_id + "&response_type=token";
            Console.WriteLine("Authenticate here and then give me back the access token.");
            Console.WriteLine(authurl);
            string access_token = Console.ReadLine();

            CdsService cds = new CdsService(org_name, access_token);

            string Setter_OPI = "cra64_onboardingprogressindicators";
            string Setter_OnboardingChecklist = "cra64_employeeonboardingchecklists";
            string Setter_User = "systemusers";

            //First delete all of the OPI's
            JObject[] AllOpis = cds.GetRecordsAsync(Setter_OPI).Result;
            Console.WriteLine(AllOpis.Length.ToString() + " found");
            foreach (JObject jo in AllOpis)
            {
                string this_opi_id = jo["cra64_onboardingprogressindicatorid"].ToString();
                Console.WriteLine("Deleting OPI " + this_opi_id);
                cds.DeleteRecordAsync(Setter_OPI, this_opi_id).Wait();
            }



            //get all of the onboarding checklists
            Console.WriteLine("Getting all Onboarding checklists...");
            JObject[] OnboardingChecklists = cds.GetRecordsAsync(Setter_OnboardingChecklist).Result;
            foreach (JObject jo in OnboardingChecklists)
            {
                string this_oc_guid = jo["cra64_employeeonboardingchecklistid"].ToString();
                Console.WriteLine("Working on Checklist " + this_oc_guid);

                //Get the user for this onboarding checklist 
                JObject this_user = cds.GetRecordAsync(Setter_User, jo["_cra64_onboardingemployee_value"].ToString()).Result;

                //Get the users vals
                string Final_Name = this_user["fullname"].ToString();
                string Final_PicUrl = "";
                if (this_user["cra64_profilepictureurl"] != null)
                {
                    Final_PicUrl = this_user["cra64_profilepictureurl"].ToString();
                }
                    
                //Get IT steps remaining
                int Final_ItStepsRemaining = 0;
                if ((bool)jo["cra64_inventorylaptopdesktop"] == false)
                {
                    Final_ItStepsRemaining = Final_ItStepsRemaining + 1;
                }
                if ((bool)jo["cra64_setupphone"] == false)
                {
                    Final_ItStepsRemaining = Final_ItStepsRemaining + 1;
                }
                if ((bool)jo["cra64_deploylaptopdesktop"] == false)
                {
                    Final_ItStepsRemaining = Final_ItStepsRemaining + 1;
                }

                //Get HR steps remainign
                int Final_HrStepsRemaining = 0;
                if ((bool)jo["cra64_addusertomfpaddressbook"] == false)
                {
                    Final_HrStepsRemaining = Final_HrStepsRemaining + 1;
                }
                if ((bool)jo["cra64_addedusertogigatrak"] == false)
                {
                    Final_HrStepsRemaining = Final_HrStepsRemaining + 1;
                }
                if ((bool)jo["cra64_itaccessrequested"] == false)
                {
                    Final_HrStepsRemaining = Final_HrStepsRemaining + 1;
                }
                if ((bool)jo["cra64_distributionlistsapplied"] == false)
                {
                    Final_HrStepsRemaining = Final_HrStepsRemaining + 1;
                }
                if ((bool)jo["cra64_samsaccountcreated"] == false)
                {
                    Final_HrStepsRemaining = Final_HrStepsRemaining + 1;
                }
                if ((bool)jo["cra64_verifiedknowbe4sync"] == false)
                {
                    Final_HrStepsRemaining = Final_HrStepsRemaining + 1;
                }

                //Get files remaining
                int Final_FilesRemaining = 0;
                if (jo["_cra64_domesticviolenceagreement_value"].ToString() != "")
                {
                    Final_FilesRemaining = Final_FilesRemaining + 1;
                }
                if (jo["_cra64_flsastatusform_value"].ToString() != "")
                {
                    Final_FilesRemaining = Final_FilesRemaining + 1;
                }
                if (jo["_cra64_i9form_value"].ToString() != "")
                {
                    Final_FilesRemaining = Final_FilesRemaining + 1;
                }
                if (jo["_cra64_policyandproceduresacknowledgement_value"].ToString() != "")
                {
                    Final_FilesRemaining = Final_FilesRemaining + 1;
                }
                if (jo["_cra64_systemconfidentialityagreement_value"].ToString() != "")
                {
                    Final_FilesRemaining = Final_FilesRemaining + 1;
                }


                //Construct a new OPI
                JObject OPI = new JObject();
                OPI["cra64_employeename"] = Final_Name;
                OPI["cra64_employeeimageurl"] = Final_PicUrl;
                OPI["cra64_itstepsremaining"] = Final_ItStepsRemaining;
                OPI["cra64_hrstepsremaining"] = Final_HrStepsRemaining;
                OPI["cra64_fileuploadsremaining"] = Final_FilesRemaining;
                OPI["cra64_ReferencedChecklist@odata.bind"] = Setter_OnboardingChecklist + "(" + this_oc_guid + ")";

                //Save the new opi
                Console.WriteLine("Creating OPI for " + Final_Name);
                cds.CreateRecordAsync(Setter_OPI, OPI).Wait();
            }



        }

    }
}
