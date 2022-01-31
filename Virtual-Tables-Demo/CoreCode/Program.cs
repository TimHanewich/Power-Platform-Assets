using System;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace CoreCode
{
    class Program
    {
        static void Main(string[] args)
        {
            RateRequest rr = new RateRequest();
            string json = RateRequest.ToJson(new RateRequest[]{rr}).ToString();
            Console.WriteLine(json);
        }
    }
}
