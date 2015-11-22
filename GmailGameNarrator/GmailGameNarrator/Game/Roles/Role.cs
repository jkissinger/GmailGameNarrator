using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailGameNarrator.Game.Roles
{
    abstract class Role
    {
        abstract public string Name { get; }
        abstract public Team Team { get; }
        /// <summary>
        /// The maximum number of players with this role in a game.
        /// </summary>
        abstract public int MaxPlayers { get; }
        /// <summary>
        /// Likelihood of assigning this role, to make the more fun roles more likely to be picked.
        /// </summary>
        abstract public int Priority { get; }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return obj.ToString().Equals(ToString());
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
