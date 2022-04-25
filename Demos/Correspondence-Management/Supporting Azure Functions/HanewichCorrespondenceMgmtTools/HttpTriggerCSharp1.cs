using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage;
using System.Text;

namespace Company.Function
{
    public static class HttpTriggerCSharp1
    {
        [FunctionName("HttpTriggerCSharp1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("SignDocument")]
        public static HttpResponseMessage SignDocument([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req, ILogger logger)
        {
            //Get content
            StreamReader sr = new StreamReader(req.Body);
            string content = sr.ReadToEnd();

            SignRequestPayload srp = JsonConvert.DeserializeObject<SignRequestPayload>(content);

            //Load the document
            byte[] doc_bytes = Convert.FromBase64String(srp.Document);
            Stream doc = new MemoryStream(doc_bytes);

            //Sign 1
            if (srp.Signature1 != null && srp.Person1 != null)
            {
                byte[] sig1_bytes = Convert.FromBase64String(srp.Signature1);
                MemoryStream sig1ms = new MemoryStream(sig1_bytes);
                Stream after = AddSignature(doc, sig1ms, srp.Person1);
                doc = after;
            }

            //Sign 2
            if (srp.Signature2 != null && srp.Person2 != null)
            {
                byte[] sig2_bytes = Convert.FromBase64String(srp.Signature2);
                MemoryStream sig2ms = new MemoryStream(sig2_bytes);
                Stream after = AddSignature(doc, sig2ms, srp.Person2);
                doc = after;
            }

            //Sign 3
            if (srp.Signature3 != null && srp.Person3 != null)
            {
                byte[] sig3_bytes = Convert.FromBase64String(srp.Signature3);
                MemoryStream sig3ms = new MemoryStream(sig3_bytes);
                Stream after = AddSignature(doc, sig3ms, srp.Person3);
                doc = after;
            }

            //Sign 4
            if (srp.Signature4 != null && srp.Person4 != null)
            {
                byte[] sig4_bytes = Convert.FromBase64String(srp.Signature4);
                MemoryStream sig4ms = new MemoryStream(sig4_bytes);
                Stream after = AddSignature(doc, sig4ms, srp.Person4);
                doc = after;
            }

            //Prepare the response
            HttpResponseMessage hrm = new HttpResponseMessage();
            hrm.StatusCode = HttpStatusCode.OK;
            hrm.Content = new StreamContent(doc);
            return hrm;
            
        }

        [FunctionName("SignDocumentReturnBase64")]
        public static HttpResponseMessage SignDocumentReturnBase64([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req, ILogger logger)
        {
            //Get content
            StreamReader sr = new StreamReader(req.Body);
            string content = sr.ReadToEnd();

            SignRequestPayload srp = JsonConvert.DeserializeObject<SignRequestPayload>(content);

            //Load the document
            byte[] doc_bytes = Convert.FromBase64String(srp.Document);
            Stream doc = new MemoryStream(doc_bytes);

            //Sign 1
            if (srp.Signature1 != null && srp.Person1 != null)
            {
                byte[] sig1_bytes = Convert.FromBase64String(srp.Signature1);
                MemoryStream sig1ms = new MemoryStream(sig1_bytes);
                Stream after = AddSignature(doc, sig1ms, srp.Person1);
                doc = after;
            }

            //Sign 2
            if (srp.Signature2 != null && srp.Person2 != null)
            {
                byte[] sig2_bytes = Convert.FromBase64String(srp.Signature2);
                MemoryStream sig2ms = new MemoryStream(sig2_bytes);
                Stream after = AddSignature(doc, sig2ms, srp.Person2);
                doc = after;
            }

            //Sign 3
            if (srp.Signature3 != null && srp.Person3 != null)
            {
                byte[] sig3_bytes = Convert.FromBase64String(srp.Signature3);
                MemoryStream sig3ms = new MemoryStream(sig3_bytes);
                Stream after = AddSignature(doc, sig3ms, srp.Person3);
                doc = after;
            }

            //Sign 4
            if (srp.Signature4 != null && srp.Person4 != null)
            {
                byte[] sig4_bytes = Convert.FromBase64String(srp.Signature4);
                MemoryStream sig4ms = new MemoryStream(sig4_bytes);
                Stream after = AddSignature(doc, sig4ms, srp.Person4);
                doc = after;
            }

            //Turn it into base 64
            doc.Position = 0;
            MemoryStream msfinal = new MemoryStream();
            doc.CopyTo(msfinal);
            byte[] bytesforfinal = msfinal.ToArray();
            string as_base_64 = Convert.ToBase64String(bytesforfinal);

            //Prepare the response
            HttpResponseMessage hrm = new HttpResponseMessage();
            hrm.StatusCode = HttpStatusCode.OK;
            hrm.Content = new StringContent(as_base_64);
            return hrm;
            
        }

        [FunctionName("SignDocumentLight")]
        public static async Task<HttpResponseMessage> SignDocumentLight([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req, ILogger logger)
        {
            //Get content
            StreamReader sr = new StreamReader(req.Body);
            string content = sr.ReadToEnd();
            SignRequestPayloadV2 srp = JsonConvert.DeserializeObject<SignRequestPayloadV2>(content);

            HttpClient hc = new HttpClient();

            //Get the document
            HttpResponseMessage hrm = await hc.GetAsync(srp.DocumentUrl);
            Stream str_doc = await hrm.Content.ReadAsStreamAsync();

            //If sig 1
            if (srp.Signature1Url != null && srp.Person1 != null)
            {
                HttpResponseMessage hrm_sig = await hc.GetAsync(srp.Signature1Url);
                Stream str_sig = await hrm_sig.Content.ReadAsStreamAsync();
                str_doc = AddSignature(str_doc, str_sig, srp.Person1);
            }

            //If sig 2
            if (srp.Signature2Url != null && srp.Person2 != null)
            {
                HttpResponseMessage hrm_sig = await hc.GetAsync(srp.Signature2Url);
                Stream str_sig = await hrm_sig.Content.ReadAsStreamAsync();
                str_doc = AddSignature(str_doc, str_sig, srp.Person2);
            }

            //If sig 3
            if (srp.Signature3Url != null && srp.Person3 != null)
            {
                HttpResponseMessage hrm_sig = await hc.GetAsync(srp.Signature3Url);
                Stream str_sig = await hrm_sig.Content.ReadAsStreamAsync();
                str_doc = AddSignature(str_doc, str_sig, srp.Person3);
            }

            //If sig 4
            if (srp.Signature4Url != null && srp.Person4 != null)
            {
                HttpResponseMessage hrm_sig = await hc.GetAsync(srp.Signature4Url);
                Stream str_sig = await hrm_sig.Content.ReadAsStreamAsync();
                str_doc = AddSignature(str_doc, str_sig, srp.Person4);
            }

            //Upload the completely signed document to azure blob storage
            CloudStorageAccount csa;
            CloudStorageAccount.TryParse("DefaultEndpointsProtocol=https;AccountName=cormgmt;AccountKey=Ix6qEiUtfI50IYyBHYM0fF7I+fsSCResG9LVyZY0EiQXetq2odF3QXHGvgR/iBQmZWWrm1zqek/IwNZ9LneQHQ==;EndpointSuffix=core.windows.net", out csa);
            CloudBlobClient cbc = csa.CreateCloudBlobClient();
            CloudBlobContainer cont = cbc.GetContainerReference("signed");
            await cont.CreateIfNotExistsAsync();
            CloudBlockBlob blb = cont.GetBlockBlobReference(Guid.NewGuid() + ".jpg");
            blb.UploadFromStream(str_doc);

            //Create an SAS url for it
            SharedAccessBlobPolicy policy = new SharedAccessBlobPolicy();
            policy.Permissions = SharedAccessBlobPermissions.Read;
            policy.SharedAccessExpiryTime = DateTime.UtcNow.AddYears(50);

            string sas = blb.GetSharedAccessSignature(policy);
            string URL = "https://cormgmt.blob.core.windows.net/signed/" + blb.Name + sas;

            //Return it
            HttpResponseMessage ToReturn = new HttpResponseMessage();
            ToReturn.StatusCode = HttpStatusCode.OK;
            string As_JSON = JsonConvert.SerializeObject(URL);
            ToReturn.Content = new StringContent(As_JSON, Encoding.UTF8, "application/json");
            return ToReturn;

        }

        public class SignRequestPayload
        {
            public string Document {get; set;} 
            public string Signature1 {get; set;} 
            public string Signature2 {get; set;} 
            public string Signature3 {get; set;} 
            public string Signature4 {get; set;} 
            public string Person1 {get; set;}
            public string Person2 {get; set;}
            public string Person3 {get; set;}
            public string Person4 {get; set;}
        }

        public class SignRequestPayloadV2
        {
            public string DocumentUrl {get; set;} 
            public string Signature1Url {get; set;} 
            public string Signature2Url {get; set;} 
            public string Signature3Url {get; set;} 
            public string Signature4Url {get; set;} 
            public string Person1 {get; set;}
            public string Person2 {get; set;}
            public string Person3 {get; set;}
            public string Person4 {get; set;}
        }

//////TOOLS BELOW
        private static Stream AddSignature(Stream original, Stream signature, string name)
        {

            //Load the images
            Image idoc = Image.FromStream(original);
            Image isig = Image.FromStream(signature);

            //Lengthen the canvas for signing
            Bitmap bm = new Bitmap(idoc.Width, idoc.Height + isig.Height + 30);
            
            //Fill it all in as white
            for (int w = 0; w < bm.Width; w++)
            {
                for (int h = 0; h < bm.Height; h++)
                {
                    bm.SetPixel(w, h, Color.White);
                }
            }

            //Draw the original doc
            Graphics g = Graphics.FromImage(bm);
            g.DrawImage(idoc, new Point(0, 0)); //Draw the original document over the new canvas.

            //Get the indent location
            float il = (float)idoc.Width * 0.1f;
            int il_int = Convert.ToInt32(il);

            //Write in the name of the person
            g.DrawString(name, new Font("Times New Roman", 16, FontStyle.Bold, GraphicsUnit.Pixel), Brushes.Blue, new PointF(il_int, idoc.Height + 10));

            //Write the signature
            g.DrawImage(isig, new Point(il_int, idoc.Height + 30));

            //Save and return
            MemoryStream fms = new MemoryStream();
            bm.Save(fms, ImageFormat.Jpeg);
            fms.Position = 0;
            return fms;
        }

    }
}
