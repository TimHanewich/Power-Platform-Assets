using System;
using PSJ.AzureSpeechToText;
using Newtonsoft.Json;
using TimHanewich.Cds;
using Newtonsoft.Json.Linq;

namespace PSJ
{
    public class Program
    {
        public static void Main(string[] args)
        {            
            CaseTestimony[] testimonies = InvestigationTools.RetrieveAndPrepareTestimoniesAsync(new Guid[]{Guid.Parse("7cf84f84-f211-ee11-8f6d-0022481c8ac0"), Guid.Parse("29ff0b17-f311-ee11-8f6d-0022481c8ac0")}).Result;
            string comp = InvestigationTools.CompareTestimoniesAsync(testimonies).Result;
            Console.WriteLine(comp);

        }
    }
}