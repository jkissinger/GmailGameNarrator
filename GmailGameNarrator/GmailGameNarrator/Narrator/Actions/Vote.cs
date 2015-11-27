using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailGameNarrator.Narrator
{
    public class Vote : Action
    {
        public Player Candidate { get; set; }

        public Vote(Action action, Player candidate) : base(action.Name, action.Parameter)
        {
            Candidate = candidate;
        }
    }
}
