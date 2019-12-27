using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using WebHook.GitHub;

namespace tests
{
    [TestClass]
    public class GitHubIssueEventTest
    {
        [TestMethod]
        public void GitHubIssueOpened()
        {

        }

        [TestMethod]
        public void GitHubIssueClosed()
        {

        }

        [TestMethod]
        public void GitHubIssueCommentDeleted()
        {

        }

        [TestMethod]
        public void GitHubIssueUserAssigned()
        {

        }

        [TestMethod]
        public void GitHubIssueUserUnAssigned()
        {

        }
        [TestMethod]
        public void GitHubIssueLabelAssigned()
        {

        }
        [TestMethod]
        public void GitHubIssueLabelUnAssigned()
        {

        }


        [TestMethod]
        public void GitHubIssueEventCommentCreate()
        {
            GitHubIssueEvent issueEvent = JsonConvert.DeserializeObject<GitHubIssueEvent>(File.ReadAllText(@"GitHubIssueEvent.json"));

            Assert.AreEqual(issueEvent.Action, GitHubIssueActions.Actions.opened.ToString());

            // Check if GitHubIssue is created.
            Assert.IsNotNull(issueEvent.Issue);
        }
    }
}
