using System;

namespace WebHook.GitHub
{
    public class GitHubIssueCommentActions
    {
                public enum Actions
        {
            created,
            edited,
            deleted,
            undefined
        }

        private Actions _action;

        public override string ToString()
        {
            return Enum.GetName(typeof(Actions), _action);
        }


        public GitHubIssueCommentActions(string action)
        {

            _action = (Actions)Enum.Parse(typeof(Actions), action);

        }
    }


}