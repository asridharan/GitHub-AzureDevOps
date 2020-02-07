using System;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace WebHook.GitHub
{
    public class AzDevOpsController
    {
        private WorkItemTrackingHttpClient _witClient;
        private ILogger _log;

        private static string WebHookStorageConnString = System.Environment.GetEnvironmentVariable("WEBSITE_CONTENTAZUREFILECONNECTIONSTRING");

        public AzDevOpsController(ILogger log)
        {
            Uri orgUrl = new Uri(System.Environment.GetEnvironmentVariable("AZURE_DEVOPS_ORG_URL"));
            String personalAccessToken = System.Environment.GetEnvironmentVariable("AzDevOpsPersonalAccessToken");
            // Create a connection
            VssConnection azDevOpsConnection = new VssConnection(orgUrl, new VssBasicCredential(string.Empty, personalAccessToken));

            // Get an instance of the work item tracking client
            _witClient = azDevOpsConnection.GetClient<WorkItemTrackingHttpClient>();
            _log = log;
        }

        public WorkItem GetWorkItem(int WorkItemId)
        {
            try
            {
                // Get the specified work item
                WorkItem workitem = _witClient.GetWorkItemAsync(WorkItemId).GetAwaiter().GetResult();

                return workitem;
            }
            catch (AggregateException aex)
            {
                VssServiceException vssex = aex.InnerException as VssServiceException;
                if (vssex != null)
                {
                    _log.LogInformation(vssex.Message);
                }
            }

            return null;

        }

        private JsonPatchDocument generateWorkItemRequest(Ticket ticket)
        {
            JsonPatchDocument patchDocument = new JsonPatchDocument();

            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Title",
                    Value = ticket.Title,
                }
            );

            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Description",
                    Value = $"{ticket.Description}{Environment.NewLine}  <div><b>GitHub:<b> <a href=\"{ticket.GitHubIssueURI}\">{ticket.GitHubIssueURI} </a></div>",
                }
            );

            /*patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.History",
                    Value = $"{ticket.Assignee} has the most context around this."
                }
            );*/

            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.AssignedTo",
                    Value = ticket.Assignee
                }
            );

            //Should be done only when there is a parten work item presented.
            if (ticket.AzDevOpsParentURI != null){
             patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/relations/-",
                    Value = new
                    {
                        rel = "System.LinkTypes.Hierarchy-Reverse",
                        url = ticket.AzDevOpsParentURI,
                        attributes = new
                        {
                            comment = "decomposition of work"
                        }
                    }
                }
            );
        }


            return patchDocument;


        }

        public WorkItem CreateWorkItem(Ticket ticket)
        {
            var patchDocument = generateWorkItemRequest(ticket);
            
            return _witClient.CreateWorkItemAsync(patchDocument, "One", "Task").GetAwaiter().GetResult();
        }

        public WorkItem UpdateWorkItem(Ticket ticket)
        {
            if (ticket.AzDevOpsWorkItemID is null) {
                throw new ArgumentException("Work Item ID cannot be null, when updating a work item in AzDevOps.");
            }

            var patchDocument = generateWorkItemRequest(ticket);
            return _witClient.UpdateWorkItemAsync(patchDocument, (int) ticket.AzDevOpsWorkItemID, false).GetAwaiter().GetResult();
        }

    }
}