using System.Collections.Generic;
using Functional.Option;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace WebHook.GitHub
{
    public interface IStorage
    {
        Option<Ticket> GetTicket(GitHubIssue issue);
        Option<Ticket> CreateTicket(GitHubIssue issue, Option<WorkItem> workItem);
        Option<Ticket> UpdateTicket(Ticket ticket, GitHubIssue issue);
        Option<Ticket> UpdateTicket(Ticket ticket, WorkItem workItem);
    }
}