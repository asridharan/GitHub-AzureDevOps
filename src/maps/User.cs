using Newtonsoft.Json;

namespace WebHook.GitHub
{
    public class User
    {
        [JsonProperty("githubuser")]
        public string GitHub
        { get; set; }

        [JsonProperty("azuredevopsuser")]
        public string AzureDevops
        { get; set; }
    }
}