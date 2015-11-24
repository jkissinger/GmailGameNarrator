using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailGameNarrator.Game.Roles
{
    class Healer : Sheeple
    {
        //GAME Finish implementing healer
        public override string Name
        {
            get
            {
                return "Doctor";
            }
        }

        public override int MaxPercentage
        {
            get
            {
                return 25;
            }
        }

        public override int MaxPlayers
        {
            get
            {
                return 1;
            }
        }
    }
}
