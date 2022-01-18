using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoreCode
{
    public class RateRequest
    {
        public Guid Id {get; set;} //Primary key
        public string CompanyName {get; set;}
        public DateTime FilingDate {get; set;}
        public FilingType FilingType {get; set;}
        public string SerffTrNumber {get; set;}
        public RateRequestStatus StateStatus {get; set;}
        public float IncreaseFiled {get; set;}
        public float IncreaseApproved {get; set;}

        public static RateRequest[] All()
        {
            List<RateRequest> ToReturn = new List<RateRequest>();

            //1
            RateRequest rr = new RateRequest();
            rr.Id = Guid.Parse("ec086998-ce51-4569-b81c-68cc51687d09");
            rr.CompanyName = "UnitedHealthCare Insurance Company";
            rr.FilingDate = new DateTime(2021, 4, 2);
            rr.FilingType = FilingType.FormRate;
            rr.SerffTrNumber = "UHLC-132771726";
            rr.StateStatus = RateRequestStatus.Disapproved;
            rr.IncreaseFiled = 9.6f;
            rr.IncreaseApproved = 0f;
            ToReturn.Add(rr);

            //2
            rr = new RateRequest();
            rr.Id = Guid.Parse("8b30321c-b3c5-49aa-9ca6-f1cd3343743f");
            rr.CompanyName = "Cigna Health and Life Insurance Company";
            rr.FilingDate = new DateTime(2021, 4, 7);
            rr.FilingType = FilingType.FormRate;
            rr.SerffTrNumber = "CCCH-132775664";
            rr.StateStatus = RateRequestStatus.Disapproved;
            rr.IncreaseFiled = 0f;
            rr.IncreaseApproved = 0f;
            ToReturn.Add(rr);

            //3
            rr = new RateRequest();
            rr.Id = Guid.Parse("aa43b6dc-5d34-4983-8325-3bfaa646e1ec");
            rr.CompanyName = "UnitedHealthCare Insurance Company";
            rr.FilingDate = new DateTime(2021, 6, 9);
            rr.FilingType = FilingType.FormRate;
            rr.SerffTrNumber = "UHLC-132856246";
            rr.StateStatus = RateRequestStatus.Approved;
            rr.IncreaseFiled = 13.2f;
            rr.IncreaseApproved = 13.2f;
            ToReturn.Add(rr);

            //4
            rr = new RateRequest();
            rr.Id = Guid.Parse("2ab3e65c-eea4-4b25-b347-4411be5651d2");
            rr.CompanyName = "UnitedHealthCare of New Mexico";
            rr.FilingDate = new DateTime(2021, 6, 10);
            rr.FilingType = FilingType.FormRate;
            rr.SerffTrNumber = "UHLC-132860025";
            rr.StateStatus = RateRequestStatus.Approved;
            rr.IncreaseFiled = 0f;
            rr.IncreaseApproved = 0f;
            ToReturn.Add(rr);

            //5
            rr = new RateRequest();
            rr.Id = Guid.Parse("947ec02e-12cf-4daf-981d-d70bbb8845c3");
            rr.CompanyName = "Friday Health Plans of Colorado, Inc.";
            rr.FilingDate = new DateTime(2021, 6, 11);
            rr.FilingType = FilingType.FormRate;
            rr.SerffTrNumber = "COHP-132820405";
            rr.StateStatus = RateRequestStatus.ApprovedCertified;
            rr.IncreaseFiled = 2.470f;
            rr.IncreaseApproved = 2.470f;
            ToReturn.Add(rr);

            //6
            rr = new RateRequest();
            rr.Id = Guid.Parse("829d7fce-8db8-4886-a41a-1f0858f806b8");
            rr.CompanyName = "Friday Health Plans of Colorado, Inc.";
            rr.FilingDate = new DateTime(2021, 6, 11);
            rr.FilingType = FilingType.FormRate;
            rr.SerffTrNumber = "COHP-132820481";
            rr.StateStatus = RateRequestStatus.Approved;
            rr.IncreaseFiled = 2.470f;
            rr.IncreaseApproved = 2.470f;
            ToReturn.Add(rr);

            //7
            rr = new RateRequest();
            rr.Id = Guid.Parse("05ab0720-7ffc-4d28-893b-c2b7f1f7fb7e");
            rr.CompanyName = "Friday Health Plans of Colorado, Inc.";
            rr.FilingDate = new DateTime(2021, 6, 11);
            rr.FilingType = FilingType.FormRate;
            rr.SerffTrNumber = "COHP-132820517";
            rr.StateStatus = RateRequestStatus.Approved;
            rr.IncreaseFiled = 10.510f;
            rr.IncreaseApproved = 10.510f;
            ToReturn.Add(rr);

            //8
            rr = new RateRequest();
            rr.Id = Guid.Parse("425fdb4d-0021-4450-9a44-cc3f9c10e3ac");
            rr.CompanyName = "Western Sky Community Care, Inc.";
            rr.FilingDate = new DateTime(2021, 6, 11);
            rr.FilingType = FilingType.FormRate;
            rr.SerffTrNumber = "CECO-132799464";
            rr.StateStatus = RateRequestStatus.ApprovedCertified;
            rr.IncreaseFiled = -4.170f;
            rr.IncreaseApproved = -4.170f;
            ToReturn.Add(rr);

            //9
            rr = new RateRequest();
            rr.Id = Guid.Parse("e19c5d29-8e26-49cb-80c0-c3429fba569f");
            rr.CompanyName = "Western Sky Community Care, Inc.";
            rr.FilingDate = new DateTime(2021, 6, 11);
            rr.FilingType = FilingType.FormRate;
            rr.SerffTrNumber = "CECO-132799468";
            rr.StateStatus = RateRequestStatus.Approved;
            rr.IncreaseFiled = -4.170f;
            rr.IncreaseApproved = -4.170f;
            ToReturn.Add(rr);


            return ToReturn.ToArray();
        }
    
        public static string ToJson(RateRequest rr)
        {
            string ToReturn = JsonConvert.SerializeObject(rr);
            return ToReturn;
        }

        public static string ToJson(RateRequest[] rrs)
        {
            List<JObject> ToReturn = new List<JObject>();
            foreach (RateRequest rr in All())
            {
                ToReturn.Add(JObject.Parse(ToJson(rr)));
            }
            return JsonConvert.SerializeObject(ToReturn.ToArray());
        }
    }
}