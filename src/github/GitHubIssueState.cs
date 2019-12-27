using System;

namespace WebHook.GitHub
{
    public class GitHubIssueState
    {
        public enum State 
        {
           open,
           closed 

        }

        private State _state;

        public override string ToString()
        {
            return Enum.GetName(typeof(State), _state);
        }


        public GitHubIssueState(string action)
        {

            _state = (State)Enum.Parse(typeof(State), action);

        }
    }
}