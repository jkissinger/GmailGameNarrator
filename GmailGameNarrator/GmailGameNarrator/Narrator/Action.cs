using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailGameNarrator.Narrator
{
    public class Action
    {
        public GameSystem.ActionEnum Type { get; }
        //Make this a player, add other options?
        public Player Target { get; set; }
        public string Text { get; set; }

        public Action(GameSystem.ActionEnum actionEnum)
        {
            Type = actionEnum;
        }

        public override string ToString()
        {
            return Type.ToString() + " " + Target.Name;
        }
    }
}
