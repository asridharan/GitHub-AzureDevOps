using Newtonsoft.Json;

namespace WebHook.GitHub
{
    public class GitHubIssueCommentEvent
    {
        [JsonProperty("issue")]
        public GitHubIssue Issue
        { get; set; }

        private GitHubIssueCommentActions _Action
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
                _Action = new GitHubIssueCommentActions(value);

            }
        }
    }

    
}