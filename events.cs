using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CounterStrikeSharp.API.Core.Listeners;

namespace better_vote
{
    public partial class better_vote
    {
        [GameEventHandler]
        public HookResult Event_RoundEnd(EventRoundEnd @event, GameEventInfo info)
        {
            if((VoteActive || IsRTVvoteActive) && Config.EndVotingOnRoundEnd)
            {
                StopVoting();
            }
            return HookResult.Continue;
        }

        public void OnMapStart(string mapName)
        {
            StopVoting();
        }
    }
}
