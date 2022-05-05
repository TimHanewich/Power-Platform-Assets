using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataversePerformance
{
    public class Contact
    {
        public string? FirstName {get; set;}
        public string? LastName {get; set;}
        public DateTime BirthDate {get; set;}
        public long MobilePhone {get; set;}

        public JObject ToDataversePayload()
        {
            JObject jo = new JObject();
            jo.Add("firstname", FirstName);
            jo.Add("lastname", LastName);
            jo.Add("birthdate", BirthDate.ToString("o").Replace("T00:00:00.0000000", ""));
            jo.Add("mobilephone", MobilePhone.ToString());
            return jo;
        }
    }
}