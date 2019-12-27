using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Functional.Option;

namespace WebHook.GitHub
{
    public class Ticket : TableEntity
    {
        public string Assignee
        { get; set; }

        public string Description
        { get; set; }

        public string Title
        { get; set; }

        public string AzDevOpsURI
        { get; set; }

        public int? AzDevOpsWorkItemID
        { get; set; }

        public string GitHubIssueURI
        {get; set;}

        private int _gitHubIssueId;
        public int GitHubIssueId
        {
            get => _gitHubIssueId;

            set
            {
                RowKey = value.ToString();
                _gitHubIssueId = value;
            }
        }

        private string _gitHubIssueLabel;
        public string GitHubIssueLabel
        {
            get => _gitHubIssueLabel;
            set
            {
                PartitionKey = value;
                _gitHubIssueLabel = value;
            }
        }

        public Ticket()
        {

        }

        public Ticket(GitHubIssue issue, Option<WorkItem> workItem, List<User> users)
        {
            this.UpdateGitHubIssue(issue, users);
            if (workItem.HasValue)
            {
                this.UpdateAzDevOpsWorkItem(workItem.Value);
            }
        }


        public void UpdateAzDevOpsWorkItem(Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem workItem)
        {
            AzDevOpsWorkItemID = workItem.Id;
            AzDevOpsURI = workItem.Url.ToString();
        }

        public void UpdateGitHubIssue(GitHubIssue issue, List<User> users)
        {

            var gitHubAssignee = issue.GetAssignee();

            // If there is a GitHub assignee find the corresponding AzDevOps Assignee.
            gitHubAssignee.Match(
                None: () => { },
                Some: value =>
                {
                // Walk through the list of GitHub user -> AzDevOps user map and select the one that matches our GitHub assignee.
                var assignees = users.Where(user => user.GitHub.Equals(value));

                    if (!(assignees is null))
                    {
                        Assignee = assignees.First().AzureDevops;
                    }
                }
            );

            Description = issue.Body;
            Title = issue.Title;
            GitHubIssueLabel = issue.GetAzDevOpsLabel();
            GitHubIssueId = issue.Number;
            GitHubIssueURI = issue.Url;
        }
    }
}