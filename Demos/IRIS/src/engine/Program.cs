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
            InvestigationTools.GenerateSuspectDrawingsAsync(Guid.Parse("4081651b-b266-ee11-8def-001dd80bf6ae")).Wait();
        }
    }
}