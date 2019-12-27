using System;

namespace WebHook.GitHub
{
    public class GitHubIssueActions
    {
        public enum Actions
        {
            opened,
            edited,
            deleted,
            pinned,
            unpinned,
            closed,
            reopened,
            assigned,
            unassigned,
            labeled,
            unlabeled,
            locked,
            unlocked,

            transfered,
            milestoned,
            demilestoned,
            undefined
        }

        private Actions _action;

        public override string ToString()
        {
            return Enum.GetName(typeof(Actions), _action);
        }


        public GitHubIssueActions(string action)
        {

            _action = (Actions)Enum.Parse(typeof(Actions), action);

        }
    }
}