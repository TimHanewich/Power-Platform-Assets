using System;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace CoreCode
{
    class Program
    {
        static void Main(string[] args)
        {
            ApiCallLogToolkit.UploadApiCallLogAsync("Hiiii!").Wait();
        }
    }
}
