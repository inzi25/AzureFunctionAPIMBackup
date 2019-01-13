using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace apimdev
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([TimerTrigger("0 0 23 * * 4")]TimerInfo myTimer, TraceWriter log)
        {
            var reply = ApimDevBackup();
            log.Info(reply.Result);
            Console.ReadLine();

            log.Info($"successfully backedup: {DateTime.Now}");
        }

        private static async Task<String> GetToken()
        {
            string clientID = "xxxxxxxxxxxxxxxxxxxxxx";
            string username = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
            string password = "xxxxxxxxxxxxxxxxxxxxxxxxxx";

            var authenticationContext = new AuthenticationContext("https://login.microsoftonline.com/xxxxxxxxxxxxxxxxxxx");
            var credential = new UserPasswordCredential(username, password);

            var result = await authenticationContext.AcquireTokenAsync("https://management.azure.com/", clientID, credential);

            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }

            return result.AccessToken;
        }


        private static async Task<String> ApimDevBackup()
        {
            var uri = "xxxxxx";

            string body = new JavaScriptSerializer().Serialize(new
            {

                storageAccount = "xxxxxxxx",
                accessKey = "xxxxxxxxxxxxxxxxxxxxxxxx",
                containerName = "xxxxxxxxxxxxxxxxxxxxxx",
                backupName = "apim_backup - " + DateTime.Now.ToString("dd/MM/yyyy")

            });


            var client = new HttpClient();
            client.BaseAddress = new System.Uri(uri);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetToken());
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            System.Net.Http.HttpContent content = new StringContent(body, UTF8Encoding.UTF8, "application/json");
            HttpResponseMessage messge = client.PostAsync(uri, content).Result;


            if (messge.IsSuccessStatusCode)
            {
                using (content = messge.Content)
                {
                    // Read the string and find the reservation id.
                    return "success";
                }
            }
            else
            {
                return await messge.Content.ReadAsStringAsync();
            }
        }
    }
}
