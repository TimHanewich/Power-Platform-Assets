using System;

namespace CoreCode
{
    public class RateRequest
    {
        public Guid Id {get; set;} //Primary key
        public string CompanyName {get; set;}
        public DateTime FilingDate {get; set;}
        public FilingType FilingType {get; set;}
        public string SerffTrNumber {get; set;}
        public float IncreaseFiled {get; set;}
        public float IncreaseApproved {get; set;}
    }
}