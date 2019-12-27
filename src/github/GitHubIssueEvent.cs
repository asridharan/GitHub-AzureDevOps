using Newtonsoft.Json;

namespace WebHook.GitHub
{
    public class GitHubIssueEvent
    {
        [JsonProperty("issue")]
        public GitHubIssue Issue
        { get; set; }

        private GitHubIssueActions _Action
        { get; set; }

        [JsonProperty("action")]
        public string Action
        {
            get
            {
                return _Action.ToString();
            }

            set
            {
                _Action = new GitHubIssueActions(value);
            }
        }
    }
}