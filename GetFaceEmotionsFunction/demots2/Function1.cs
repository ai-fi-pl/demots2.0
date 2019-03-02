using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace demots2
{
    public static class FaceEmotions
    {
        public static string GetImageUrl(HttpRequest req)
        {
            string imageUrl = req.Query["url"];
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            return imageUrl = imageUrl ?? data?.name;
        }

        [FunctionName("GetFaceEmotions")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var imageUrl = GetImageUrl(req);
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return new BadRequestObjectResult("Please pass an image URL on the query string or in the request body");
            }
            else
            {
                string faceUrlBase = Environment.GetEnvironmentVariable("CognitiveServicesUrlBase");
                string key = Environment.GetEnvironmentVariable("CognitiveServicesKey");

                // FaceLandmarks gives us a rectangle containing the face.
                // FaceAttributes=emotion will tell Cognitive Services to read the emotion of a face.
                string reqParams = "?returnFaceId=true&returnFaceLandmarks=false&returnFaceAttributes=emotion";

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var json = "{\"url\":\"" + $"{imageUrl}" + "\"}";

                var resp = await client.PostAsync(faceUrlBase + reqParams, new StringContent(json,
                                        Encoding.UTF8,
                                        "application/json"));

                var jsonResponse = await resp.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    return new BadRequestObjectResult("Please pass an image URL on the query string or in the request body");
                }
                else
                {
                    return new OkObjectResult(jsonResponse);
                }
                


            }
        }
    }
}
