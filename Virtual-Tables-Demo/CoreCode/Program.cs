using System;
using Newtonsoft.Json;

namespace CoreCode
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(RateRequest.ToJson(RateRequest.All(), new string[] {"Id"}));
        }
    }
}
