using Newtonsoft.Json;

namespace WebHook.GitHub
{
    public class GitHubAssignee
    {
        [JsonProperty("login")]
        public string Login;
    }
}