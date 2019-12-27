using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Primitives;
using System.Text;
using System.Security.Cryptography;
using Azure.Storage.Blobs;



using Newtonsoft.Json.Linq;

namespace WebHook.GitHub
{

    public static class AzDevOpsConnector
    {
        private static string Sha1Prefix = "sha1=";
        private static string ServiceSecret = System.Environment.GetEnvironmentVariable("GitHubConnectionString");
        private static string WebHookStorageConnString = System.Environment.GetEnvironmentVariable("WEBSITE_CONTENTAZUREFILECONNECTIONSTRING");

        [FunctionName("AzDevOpsConnectorHook")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            req.Headers.TryGetValue("X-GitHub-Event", out StringValues eventName);
            req.Headers.TryGetValue("X-Hub-Signature", out StringValues signature);
            req.Headers.TryGetValue("X-GitHub-Delivery", out StringValues delivery);
            log.LogInformation($"Event:{eventName}, Signature:{signature}, Delivery:{delivery}");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation($"Received request payload:{requestBody}");

            if (IsGithubPushAllowed(requestBody, eventName, signature))
            {
                log.LogInformation($"GitHubEventType: {eventName}");
                var reqJSON = JObject.Parse(requestBody);


                if (eventName == "issues")
                {
                    var url = (string)reqJSON["issue"]["url"];
                    var title = (string)reqJSON["issue"]["title"];
                    log.LogInformation($"[JSON] Recieved an issue from:{url} with title: {title}");
                    var issueJSON = reqJSON["issue"].ToString();
                    // Lets convert the JSON into a GitHub Issue
                    GitHubIssue issue = JsonConvert.DeserializeObject<GitHubIssue>(issueJSON);
                    log.LogInformation($"Recieved an issue from:{issue.Url} with title: {issue.Title}");

                    // Loading the users.
                    AzureStorage storage = new AzureStorage(log);
                    log.LogInformation($"Initialized Storage.");


                    State state = new State(log, storage);
                    log.LogInformation($"Initialized state.");
                    state.UpdateGitHubIssue(issue);


                    // Create the AzDevOps controller, and add the work item.

                    return (ActionResult)new OkObjectResult($"works with configured GitHub secret");
                }
                else
                {
                    return (ActionResult)new BadRequestObjectResult($"Unsupported Github event type:{eventName}");
                }
            }

            return new StatusCodeResult((int)System.Net.HttpStatusCode.Unauthorized);
        }

        static private bool IsGithubPushAllowed(string payload, string eventName, string signatureWithPrefix)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                throw new ArgumentNullException(nameof(payload));
            }
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentNullException(nameof(eventName));
            }
            if (string.IsNullOrWhiteSpace(signatureWithPrefix))
            {
                throw new ArgumentNullException(nameof(signatureWithPrefix));
            }

            if (signatureWithPrefix.StartsWith(Sha1Prefix, StringComparison.OrdinalIgnoreCase))
            {
                var signature = signatureWithPrefix.Substring(Sha1Prefix.Length);
                var secret = Encoding.ASCII.GetBytes(ServiceSecret);
                var payloadBytes = Encoding.UTF8.GetBytes(payload);

                using (var hmSha1 = new HMACSHA1(secret))
                {
                    var hash = hmSha1.ComputeHash(payloadBytes);

                    var hashString = ToHexString(hash);

                    if (hashString.Equals(signature))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static string ToHexString(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                builder.AppendFormat("{0:x2}", b);
            }

            return builder.ToString();
        }
    }
}
