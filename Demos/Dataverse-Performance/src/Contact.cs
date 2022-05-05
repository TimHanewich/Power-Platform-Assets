using System;
using Newtonsoft.Json;

namespace DataversePerformance
{
    public class Contact
    {
        public string? FirstName {get; set;}
        public string? LastName {get; set;}
        public DateTime BirthDate {get; set;}
        public long MobilePhone {get; set;}
    }
}