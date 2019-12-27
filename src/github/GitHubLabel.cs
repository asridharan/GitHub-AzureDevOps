using Newtonsoft.Json;

namespace WebHook.GitHub
{
    public class GitHubLabel
    {
        [JsonProperty("name")]
        public string Name {get; set;}
    }
}