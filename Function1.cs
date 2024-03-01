using Azure;
using Azure.Core;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.Json.Nodes;
using static System.Net.WebRequestMethods;

namespace AbsorbFunction
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;
        private static string UserEmail;
        private static string FirstName;
        private static string LastName;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function("Function1")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            PostWildApricotToken().Wait();
            //var token = PostToken().Result;
            //PostCreateUser(token).Wait();
            return new OkObjectResult("Welcome to Azure Functions!");
        }
         async Task<string> PostToken()
        {
            _logger.LogInformation("In PostToken function");
            // Define the URL to which you want to send the request
            string url = "https://rest.myabsorb.com/authenticate";
            string result = "";
            // Create a new instance of HttpClient
            using (HttpClient client = new HttpClient())
            {

                // Define JSON payload
                string jsonBody = "{\"username\": \"Admin\", \"password\": \"Redefinealgo123@\", \"privateKey\": \"7d29985a-4e8c-478a-b37f-27dbf93a9659\"}";

                // Create HttpRequestMessage
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("x-api-key", "7d29985a-4e8c-478a-b37f-27dbf93a9659");
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                // Define the content of the request
                //HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                try
                {
                    // Send the POST request
                    HttpResponseMessage response = await client.SendAsync(request);

                    // Check if the request was successful (status code 200-299)
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content as string
                         result = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation("result : "+ result);
                        await PostCreateUser(result);
                        Console.WriteLine("Response: " + result);
                    }
                    else
                    {
                        _logger.LogInformation("status code : " + response.StatusCode);
                        Console.WriteLine("Error: " + response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("exception : " + ex.Message);
                    Console.WriteLine("Exception: " + ex.Message);
                }
            }
            return result;
        }
         async Task<string> PostCreateUser(string token)
        {
            _logger.LogInformation("in PostCreateUser function.");
            // Define the URL to which you want to send the request
            string url = "https://rest.myabsorb.com/users?key=0";
            string result = "";
            token = token.Trim('"', '"');
            // Create a new instance of HttpClient
            using (HttpClient client = new HttpClient())
            {

                // Define JSON payload
                //string jsonBody = "{\"username\": \"Admin\", \"password\": \"Redefinealgo123@\", \"privateKey\": \"7d29985a-4e8c-478a-b37f-27dbf93a9659\"}";

                var myObject = new
                {
                    id = Guid.NewGuid().ToString(),
                    username = UserEmail,
                    password = "Redefinealgo123@",
                    departmentId = "70b39842-02b6-4e35-b2b0-0956f3419d73",
                    firstName =FirstName,
                    lastName = LastName,
                    emailAddress = UserEmail,
                    gender = 1,
                    activeStatus = 0,
                    isLearner = true,
                    isInstructor = false,
                    isAdmin = false,
                    hasUsername = true
                };

                // Convert the object to string
                string jsonBody = JsonConvert.SerializeObject(myObject);
                // Create HttpRequestMessage
                _logger.LogInformation("user obj: " + jsonBody);
                // Define the content of the request
                //HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                try
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                    request.Headers.Add("x-api-key", "7d29985a-4e8c-478a-b37f-27dbf93a9659");
                    //request.Headers.Add("Authorization", token);
                   
                    request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                    // Send the POST request
                    HttpResponseMessage response = await client.SendAsync(request);
                    _logger.LogInformation("response: " + response);
                    // Check if the request was successful (status code 200-299)
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content as string
                        result = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Response: " + result);
                    }
                    else
                    {
                        Console.WriteLine("Error: " + response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("Exception: " + ex.ToString());
                    Console.WriteLine("Exception: " + ex.Message);
                }
            }
            return result;
        }
         async Task<string> PostWildApricotToken()
        {
            _logger.LogInformation("In PostWildApricotToken function");
            // Define the URL to which you want to send the request
            string url = "https://oauth.wildapricot.org/auth/token";
            string result = "";
            var parameters = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "scope", "auto" }
        };
            // Create a new instance of HttpClient
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Basic QVBJS0VZOnR0a3RnYmRhMmRkaTJ6M2o0ZXhmNXM3dTdzMTRxNA==");
              
                var content = new FormUrlEncodedContent(parameters);
                // Create HttpRequestMessage
                // Define the content of the request
                //HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                try
                {
                    // Send the POST request with the parameters
                    HttpResponseMessage response = await client.PostAsync(url, content);
                    _logger.LogInformation("Response:" + response);
                    // Check if the request was successful (status code 200-299)
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content as string
                        result = await response.Content.ReadAsStringAsync();
                        // Parse the JSON string
                        JObject jsonResponse = JObject.Parse(result);

                        // Extract access_token and AccountId
                        string accessToken = jsonResponse["access_token"].ToString();
                        int accountId = jsonResponse["Permissions"][0]["AccountId"].Value<int>();
                        _logger.LogInformation("Token:" + accessToken);
                        GetContacts(accountId.ToString(),accessToken).Wait();
                        Console.WriteLine("Response: " + result);
                    }
                    else
                    {
                        _logger.LogInformation("Token:" + response.StatusCode);
                        Console.WriteLine("Error: " + response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("Exception:" + ex.Message);
                    Console.WriteLine("Exception: " + ex.Message);
                }
            }
            return result;
        }
         async Task GetContacts(string AccountId, string Token)
        {
            _logger.LogInformation("In GetContacts Function");
            // URL of the API endpoint you want to access
            string apiUrl = "https://api.wildapricot.org/v2/Accounts/"+ AccountId + "/Contacts";

            // Bearer token for authorization
            string bearerToken = Token;

            // Create a new instance of HttpClient
            using (HttpClient client = new HttpClient())
            {
                // Set the authorization header with the bearer token
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

                try
                {
                    // Send the GET request
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    _logger.LogInformation("response:" + response);
                    // Check if the request was successful (status code 200-299)
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content as string
                        string responseBody = await response.Content.ReadAsStringAsync();
                        // Parse the JSON string
                        JObject jsonObject = JObject.Parse(responseBody);

                        // Extract ResultUrl and State
                        string resultUrl = (string)jsonObject["ResultUrl"];
                        _logger.LogInformation("uri:" + resultUrl);
                        string state = (string)jsonObject["State"];
                        if(state == "Waiting")
                        {
                            Thread.Sleep(2000);
                            await GetContacts(AccountId,Token);
                        }
                        else
                        {
                           await GetCallContactsUrl(resultUrl,Token);
                        }
                        Console.WriteLine("Response: " + responseBody);
                    }
                    else
                    {
                        Console.WriteLine("Error: " + response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                }
            }
        }
         async Task GetCallContactsUrl(string ResultUrl, string Token)
        {
            _logger.LogInformation("in GetCallContactsUrl function");
            // URL of the API endpoint you want to access
            string apiUrl = ResultUrl;

            // Bearer token for authorization
            string bearerToken = Token;

            // Create a new instance of HttpClient
            using (HttpClient client = new HttpClient())
            {
                // Set the authorization header with the bearer token
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

                try
                {
                    // Send the GET request
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    // Check if the request was successful (status code 200-299)
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content as string
                        string responseBody = await response.Content.ReadAsStringAsync();
                        JObject jsonObjectContacts = JObject.Parse(responseBody);
                        // Parse the JSON string
                        List<JObject> jsonObjectList = JArray.Parse(jsonObjectContacts["Contacts"].ToString()).ToObject<List<JObject>>();
                        // Initialize variables to keep track of the latest date and the corresponding object
                        DateTime latestCreationDate = DateTime.MinValue;
                        JObject latestCreationObject = null;

                        // Loop through the list of objects and extract the "Creation date" field value
                        foreach (JObject jsonObject in jsonObjectList)
                        {
                            string creationDateStr = GetFieldValue(jsonObject, "Creation date");
                            string status = (string)jsonObject["Status"];
                            if (status == "Active")
                            {
                                if (DateTime.TryParse(creationDateStr, out DateTime creationDate))
                                {
                                    if (creationDate > latestCreationDate)
                                    {
                                        latestCreationDate = creationDate;
                                        latestCreationObject = jsonObject;
                                    }
                                }
                            }
                        }
                        if (latestCreationObject != null)
                        {
                            UserEmail = (string)latestCreationObject["Email"];
                            FirstName = (string)latestCreationObject["FirstName"];
                            LastName = (string)latestCreationObject["LastName"];
                            _logger.LogInformation(UserEmail + " " + FirstName + " " + LastName);
                            await PostToken();
                        }
                            Console.WriteLine("Response: " + responseBody);
                    }
                    else
                    {
                        Console.WriteLine("Error: " + response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                }
            }
        }
         string GetFieldValue(JObject jsonObject, string fieldName)
        {
            // Extract the "FieldValues" array
            JArray fieldValues = (JArray)jsonObject["FieldValues"];

            // Iterate over the array to find the entry with the specified "FieldName"
            foreach (JToken fieldValue in fieldValues)
            {
                if (fieldValue["FieldName"]?.ToString() == fieldName)
                {
                    // Return the value if the "FieldName" matches
                    return fieldValue["Value"]?.ToString();
                }
            }

            // Return null if the field with the specified name is not found
            return null;
        }
    }
}
