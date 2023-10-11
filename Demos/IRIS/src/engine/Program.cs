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
            string r = SimpleGPT.PromptAsync("Why is the sky blue?").Result;
            Console.WriteLine(r);
        }
    }
}