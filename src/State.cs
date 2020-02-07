using Functional.Option;
using Microsoft.Extensions.Logging;

namespace WebHook.GitHub
{
    public class State
    {
       
        private ILogger _log;
        private IStorage _storage;

        private AzDevOpsController _azDevOps;

        public State(ILogger log, IStorage storage)
        {
            _log = log;

            _storage = storage;
            
            log.LogInformation("Initialized GithUb->AzureDevOps user map.");

            // Setup the Azure DevOps controller.
            _azDevOps = new AzDevOpsController(log);
        }

        public void UpdateGitHubIssue(GitHubIssue issue)
        {
            // Check with storage if GitHub issue exists.
            var azDevOpsLabel = issue.GetAzDevOpsLabel();
            if (azDevOpsLabel.HasValue)
            {
                var ticket = _storage.GetTicket(issue);

                // If ticket does not exist, create a WI in AzDevOps and create the corresponding MAP.
                if (!ticket.HasValue)
                {
                    try
                    {
                        ticket = _storage.CreateTicket(issue, Option.None);
                        var workItem = _azDevOps.CreateWorkItem(ticket.Value);
                        // NOTE: We always store VSO work items as a ticket to have more structure around the data.
                        _storage.UpdateTicket(ticket.Value, workItem);
                        
                       
                    }
                    catch (System.NullReferenceException ex)
                    {
                        _log.LogCritical($"Unable to create the Azure Dev Ops workitem corresponding to issue:{issue.Url}:{ex.Message}");
                        throw;
                    }
                }
                else
                {
                    try
                    {
                        

                        // Update the issue on the ticket since the GitHub issue details might have changed.
                        ticket = _storage.UpdateTicket(ticket.Value, issue);

                        _log.LogInformation($"Updating work item:{ticket.Value.AzDevOpsWorkItemID} for issue:{ticket.Value.GitHubIssueId}");
                        var result = _azDevOps.UpdateWorkItem(ticket.Value);

                    }
                    catch (System.NullReferenceException ex)
                    {
                        _log.LogCritical($"Unable to update the Azure Dev Ops workitem corresponding to issue:{issue.Url}:{ex.Message}");
                        throw;

                    }
                    catch (System.ArgumentException ex)
                    {
                        _log.LogCritical($"Unable to update the Azure Dev Ops workitem corresponding to issue due to non-existent work item:{issue.Url}:{ex.Message}");
                        throw;

                    }
                }

            }

        }
    }
}