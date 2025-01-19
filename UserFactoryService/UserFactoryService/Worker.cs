using Newtonsoft.Json.Linq;
using RestSharp;
using System.Diagnostics;

namespace UserFactoryService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Mevcut iþlemin bilgilerini al
            Process currentProcess = Process.GetCurrentProcess();

            // Ýþlem ID'sini al (PID)
            Console.WriteLine("Process ID (PID): " + currentProcess.Id);

            // Kullanýlabilir bellek, iþlem adý gibi diðer bilgiler
            Console.WriteLine("Process Name: " + currentProcess.ProcessName);
            while (!stoppingToken.IsCancellationRequested)
            {
                var fileContent = File.ReadAllText("source.json");
                var fileContentJObj = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(fileContent);
                var getUserEndpoint = fileContentJObj!["getUserApiEndpoint"]?.ToString();
                var postUserEndpoint = fileContentJObj!["postUserApiEndpoint"]?.ToString();
                var password = fileContentJObj!["firstPassword"]?.ToString();
                var client = new RestClient();
                var getRequest = new RestRequest(getUserEndpoint);
                var getResponse = await client.ExecuteAsync(getRequest, Method.Get);
                var getResponseJObj = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(getResponse.Content!);
                var getResponseJArray = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(getResponseJObj!["results"]?.ToString()!);
                foreach(var item in getResponseJArray!)
                {
                    var postRequestJObj = new JObject();

                    postRequestJObj.Add("email", item["email"]?.ToString());
                    postRequestJObj.Add("userName", item["login"]?["username"]?.ToString());
                    postRequestJObj.Add("password", password);
                    postRequestJObj.Add("passwordConfirm", password);
                    var postRequest = new RestRequest(postUserEndpoint);
                    postRequest.AddBody(Newtonsoft.Json.JsonConvert.SerializeObject(postRequestJObj));
                    var postResponse = await client.ExecuteAsync(postRequest, Method.Post);
                    Console.WriteLine(postResponse.StatusCode + " " +  postResponse.Content);


                }
            }
        }
    }
}
