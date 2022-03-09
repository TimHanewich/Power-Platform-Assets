using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft;
using Microsoft.Azure;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace CoreCode
{
    public class ApiCallLogToolkit
    {
        public static async Task UploadApiCallLogAsync(string content)
        {
            CloudStorageAccount csa;
            CloudStorageAccount.TryParse("DefaultEndpointsProtocol=https;AccountName=nmosi;AccountKey=hHUFdkemz7Zr+f90vmulXoeWgTqp2BxPVeZ732st3eC4sLYFGmP0Ik65otXzHfbVNVRtUo9I+UG3hGEHif5iMw==;EndpointSuffix=core.windows.net", out csa);
            CloudBlobClient cbc = csa.CreateCloudBlobClient();
            CloudBlobContainer cont = cbc.GetContainerReference("api-call-logs");
            await cont.CreateIfNotExistsAsync();

            string blbName = DateTime.UtcNow.Year.ToString("0000") + DateTime.UtcNow.Month.ToString("00") + DateTime.UtcNow.Day.ToString("00") + DateTime.UtcNow.Hour.ToString("00") + DateTime.UtcNow.Minute.ToString("00") + DateTime.UtcNow.Second.ToString("00") + "-" + Guid.NewGuid().ToString().Replace("-", "");

            CloudBlockBlob blb = cont.GetBlockBlobReference(blbName);
            await blb.UploadTextAsync(content);
        }
    }
}