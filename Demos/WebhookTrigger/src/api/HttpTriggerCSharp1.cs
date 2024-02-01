using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace PublicSafetyAPI
{
    public class HttpTriggerCSharp1
    {
        private readonly ILogger<HttpTriggerCSharp1> _logger;

        public HttpTriggerCSharp1(ILogger<HttpTriggerCSharp1> logger)
        {
            _logger = logger;
        }

        [Function("HttpTriggerCSharp1")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
