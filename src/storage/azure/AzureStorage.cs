using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Functional.Option;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace WebHook.GitHub
{
    public class AzureStorage : IStorage
    {
        private BlobClient _userBlobClient;
        private BlobClient _pbBlobClient;
        private BlobServiceClient _blobServiceClient;
        private ILogger _log;

        private CloudTable _issuesTable;

        private List<User> _users;

        private Dictionary<string, string> _productBackLog;

        public AzureStorage(ILogger log)
        {
            string connectionString = System.Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            _blobServiceClient = new BlobServiceClient(connectionString);
            _userBlobClient = _blobServiceClient.GetBlobContainerClient("users").GetBlobClient("agic-dev-users.json");
            _pbBlobClient = _blobServiceClient.GetBlobContainerClient("product-backlog").GetBlobClient("pb.json");
            _log = log;

            // Initialize the issues table as well.
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(connectionString);
            }
            catch (System.FormatException)
            {
                log.LogInformation("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                throw;
            }
            catch (System.ArgumentException)
            {
                log.LogInformation("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                throw;
            }

            // Create a table client for interacting with the table service
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            // Create a table client for interacting with the table service 
            _issuesTable = tableClient.GetTableReference("issues");

            //Set up the users.
            // We need to download the blob to stream and convert it to string in order to de-serialize it as a JSON.
            var userStream = new MemoryStream();
            var downloadInfo = _userBlobClient.DownloadAsync().GetAwaiter().GetResult();
            downloadInfo.Value.Content.CopyTo(userStream);
            var userText = Encoding.ASCII.GetString(userStream.ToArray());

            // Retrieve the Json Object from the blob.
            _users = JsonConvert.DeserializeObject<List<User>>(userText);

            //Set up the product backlog mappings.
            // We need to download the blob to stream and convert it to string in order to de-serialize it as a JSON.
            var pbStream = new MemoryStream();
            downloadInfo = _pbBlobClient.DownloadAsync().GetAwaiter().GetResult();
            downloadInfo.Value.Content.CopyTo(pbStream);
            var pbText = Encoding.ASCII.GetString(pbStream.ToArray());

            // Retrieve the Json Object from the blob.
            _productBackLog = JsonConvert.DeserializeObject<Dictionary<string, string>>(pbText);
        }



        public Option<Ticket> GetTicket(GitHubIssue issue)
        {
            Option<Ticket> ticket = Option.None;
            Option<string> issueLabel = issue.GetAzDevOpsLabel();

            if (issueLabel.HasValue)
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<Ticket>(issueLabel.Value, issue.Number.ToString());


                try
                {
                    TableResult result = _issuesTable.ExecuteAsync(retrieveOperation).GetAwaiter().GetResult();
                    ticket = result.Result as Ticket;
                }
                catch (StorageException ex)
                {
                    _log.LogInformation($"Unable to retreieve work item map from storage table:{ex.Message}, type:{ex.Data.ToString()}");
                }
            }

            return ticket;
        }

        public Option<Ticket> CreateTicket(GitHubIssue issue, Option<WorkItem> workItem)
        {

            return UpdateTicket(new Ticket(issue, _users, _productBackLog, workItem), issue);
        }

        public Option<Ticket> UpdateTicket(Ticket ticket, GitHubIssue issue)
        {
            ticket.UpdateGitHubIssue(issue, _users, _productBackLog);
            return updateTicket(ticket);
        }

        public Option<Ticket> UpdateTicket(Ticket ticket, WorkItem workItem)
        {
            ticket.UpdateAzDevOpsWorkItem(workItem);
            return updateTicket(ticket);
        }

        private Option<Ticket> updateTicket(Ticket ticket)
        {
            TableOperation updateOperation = TableOperation.InsertOrMerge(ticket);
            TableResult result = _issuesTable.ExecuteAsync(updateOperation).GetAwaiter().GetResult();

            return result.Result as Ticket;
        }
    }
}