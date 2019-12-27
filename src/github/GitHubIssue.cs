using Newtonsoft.Json;
using System.Collections.Generic;
using Functional.Option;

namespace WebHook.GitHub
{
    public class GitHubIssue
    {
        [JsonProperty("html_url")]
        public string Url
        { get; set; }

        [JsonProperty("title")]
        public string Title
        { get; set; }

        [JsonProperty("number")]
        public int Number
        { get; set; }

        [JsonProperty("body")]
        public string Body
        { get; set; }

        GitHubIssueState _State;

        [JsonProperty("state")]
        public string State
        {
            get
            {
                return _State.ToString();
            }

            set
            {
                _State = new GitHubIssueState(value);
            }
        }

        [JsonProperty("labels")]
        public List<GitHubLabel> Labels { get; set; }

        [JsonProperty("assignee")]
        public GitHubAssignee Assignee { get; set; }

        [JsonProperty("assignees")]
        public List<GitHubAssignee> Assignees { get; set; }

        public string GetAzDevOpsLabel()
        {
            foreach (GitHubLabel label in Labels)
            {
                if (label.Name.Contains("AzDevOps"))
                {
                    return label.Name;
                }
            }

            return null;
        }

        public Option<string> GetAssignee()
        {
            if (!(Assignee is null))
            {
                return Assignee.Login;
            }
            else if (Assignees.Count > 0)
            {
                return Assignees[0].Login;
            }

            return Option.None;

        }

        
    }
}