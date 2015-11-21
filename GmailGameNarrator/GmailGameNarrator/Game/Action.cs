using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailGameNarrator.Game
{
    class Action
    {
        public GameSystem.ActionEnum Name { get; }
        public string Parameter { get; }

        public Action(GameSystem.ActionEnum actionEnum, string parameter)
        {
            Name = actionEnum;
            Parameter = parameter;
        }
    }
}
