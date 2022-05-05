﻿using System;
using System.Threading.Tasks;
using TimHanewich.Cds;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataversePerformance
{
    public class Program
    {

        public static void Main()
        {
            PerformDataverseUploadAsync().Wait();
        }

        public static async Task PerformDataverseUploadAsync()
        {
            string auth = await DataverseAuthenticator.GetAccessTokenAsync();
            CdsService cds = new CdsService("https://orgde82f7a5.crm.dynamics.com/", auth);
        
            Contact[] contacts = RandomContacts(1);

            for (int t = 0; t < contacts.Length; t++)
            {
                Contact toupload = contacts[t];
                Console.Write("Uploading # " + (t+1).ToString("#,##0") + " / " + contacts.Length.ToString("#,##0") + "... ");
                await cds.CreateRecordAsync("contacts", toupload.ToDataversePayload().ToString());
                Console.WriteLine("Success!");
            }

        }
        
        private static Contact[] RandomContacts(int count)
        {
            string[] FirstNames = System.IO.File.ReadAllText(@"C:\Users\tahan\Downloads\Power-Platform-Assets\Demos\Dataverse-Performance\src\FirstNames.txt").Split(Environment.NewLine);
            string[] LastNames = System.IO.File.ReadAllText(@"C:\Users\tahan\Downloads\Power-Platform-Assets\Demos\Dataverse-Performance\src\LastNames.txt").Split(Environment.NewLine);
            string[] cities = System.IO.File.ReadAllText(@"C:\Users\tahan\Downloads\Power-Platform-Assets\Demos\Dataverse-Performance\src\cities.txt").Split(Environment.NewLine);

            Random r = new Random();
            List<Contact> ToReturn = new List<Contact>();
            for (int t = 0; t < count; t++)
            {
                Contact c = new Contact();
                c.FirstName = RandomFromArray(FirstNames);
                c.LastName = RandomFromArray(LastNames);
                c.BirthDate = RandomDOB();
                c.MobilePhone = RandomPhoneNumber();
                c.AddressCity = RandomFromArray(cities);
                c.AddressLatitude = Convert.ToSingle(r.NextDouble());
                c.AddressLongitude = Convert.ToSingle(r.NextDouble());
                ToReturn.Add(c);
            }
            return ToReturn.ToArray();
        }

        private static string RandomFromArray(string[] arr)
        {
            Random r = new Random();
            string selected = arr[r.Next(arr.Length)];
            return selected;
        }

        private static DateTime RandomDOB()
        {
            int year = RandBetween(1950, 2010);
            int month = RandBetween(1, 12);
            int day = RandBetween(1, 27);
            DateTime dob = new DateTime(year, month, day);
            return dob;
        }

        private static long RandomPhoneNumber()
        {
            Random r = new Random();
            long ub = long.Parse("9999999999");
            long tr = r.NextInt64(1000000000, ub);
            return tr;
        }

        private static int RandBetween(int min, int max)
        {
            Random r = new Random();
            int val = r.Next(min, max);
            return val;
        }

        
    }
}