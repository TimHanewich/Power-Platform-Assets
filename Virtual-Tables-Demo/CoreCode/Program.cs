using System;
using Newtonsoft.Json;

namespace CoreCode
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(JsonConvert.SerializeObject(RateRequest.All()));
        }
    }
}
