using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TimHanewich.Sql;

namespace DataversePerformance
{
    public class Contact
    {
        public string? FirstName {get; set;}
        public string? LastName {get; set;}
        public DateTime BirthDate {get; set;}
        public long MobilePhone {get; set;}
        public string? AddressCity {get; set;}
        public float AddressLatitude {get; set;}
        public float AddressLongitude {get; set;}
        public int AnnualIncome {get; set;}

        public JObject ToDataversePayload()
        {
            JObject jo = new JObject();
            jo.Add("firstname", FirstName);
            jo.Add("lastname", LastName);
            jo.Add("birthdate", BirthDate.ToString("o").Replace("T00:00:00.0000000", ""));
            jo.Add("mobilephone", MobilePhone.ToString());
            jo.Add("address1_city", AddressCity);
            jo.Add("address1_latitude", AddressLatitude);
            jo.Add("address1_longitude", AddressLongitude);
            jo.Add("annualincome", AnnualIncome);
            return jo;
        }

        public string ToSqlInsert()
        {
            InsertHelper ih = new InsertHelper("Contact");
            ih.Add("Id", Guid.NewGuid().ToString(), true);
            ih.Add("FirstName", FirstName.Replace("'", ""), true);
            ih.Add("LastName", LastName.Replace("'", ""), true);
            ih.Add("BirthDate", SqlToolkit.ToSqlDateString(BirthDate), true);
            ih.Add("MobilePhone", MobilePhone.ToString());
            ih.Add("AddressCity", AddressCity.Replace("'", ""), true);
            ih.Add("AddressLatitude", AddressLatitude.ToString());
            ih.Add("AddressLongitude", AddressLongitude.ToString());
            ih.Add("AnnualIncome", AnnualIncome.ToString());
            return ih.ToString();
        }
    }
}