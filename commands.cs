using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CounterStrikeSharp.API.Modules.Utils;
using MenunManager2 = CounterStrikeSharp.API.Modules.Menu.MenuManager;
using CS2MenuManager.API.Menu;
using CounterStrikeSharp.API.Modules.Entities;
using CS2MenuManager.API.Class;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core.Translations;
using System.Numerics;

namespace better_vote
{
    public partial class better_vote
    {
        [ConsoleCommand("css_vote")]
        public void Command_Vote(CCSPlayerController? caller, CommandInfo command)
        {
            if (!PlayerHasPermission(caller, Config.VotePermission)) return;
            if (CheckActiveVoting(caller))
            {
                reply(caller, "bv_YesVote");
                return;
            }

            if (command.ArgCount < 2)
            {
                reply(caller, "bv_WrongUsage");
                return;
            }
            if (command.ArgCount > 9)
            {
                reply(caller, "bv_MaxOptions");
                return;
            }

            Question = command.GetArg(1);
            options.Clear();
            voteData.Clear();

            if (command.ArgCount == 2)
            {
                options.Add("Yes");
                options.Add("No");
            }
            else
            {
                for (int i = 2; i < command.ArgCount; i++)
                {
                    string option = command.GetArg(i);
                    if (options.Contains(option))
                    {
                        reply(caller, "bv_DuplicateOptions", option);
                        return;
                    }
                    options.Add(option);
                }
            }
            StartVoting();
        }

        [ConsoleCommand("css_votereset")]
        [ConsoleCommand("css_votecancel")]
        [ConsoleCommand("css_votestop")]
        [ConsoleCommand("css_stopvote")]
        [ConsoleCommand("css_vote0")]
        public void Command_Vote0(CCSPlayerController? caller, CommandInfo command)
        {
            if (!PlayerHasPermission(caller, Config.VotePermission)) return;
            if (!CheckActiveVoting(caller))
            {
                reply(caller, "bv_NoVote");
                return;
            }

            VoteActive = false;
            options.Clear();
            Question = "";
            voteData.Clear();
            VotedPlayers.Clear();

            foreach (var player in Utilities.GetPlayers().Where(p => p is { IsHLTV: false, IsBot: false, IsValid: true }))
            {
                MenuManager.CloseActiveMenu(player);
                MenunManager2.CloseActiveMenu(player);
            }

            reply(caller, $"bv_VoteStopped");
        }

        [ConsoleCommand("css_revote")]
        public void Command_ReVote(CCSPlayerController? caller, CommandInfo command)
        {
            if (!CheckActiveVoting(caller))
            {
                reply(caller, "bv_NoVote");
                return;
            }

            if (caller == null || !caller.IsValid) return;

            if (VotedPlayers.ContainsKey(caller.Slot))
            {
                string previousVote = VotedPlayers[caller.Slot];
                if (voteData.ContainsKey(previousVote))
                {
                    voteData[previousVote]--;
                    if (voteData[previousVote] <= 0)
                    {
                        voteData.Remove(previousVote);
                    }
                }

                var image = "<img src='https://raw.githubusercontent.com/vulikit/better-vote/refs/heads/main/resources/votebox.png'>";
                WasdMenu wasd_votemenu = new WasdMenu($"<font color='#FFF085' class='fontSize-l'>{image} {Question} {image}</font>", this);
                ScreenMenu s_votemenu = new ScreenMenu($"{Question}", this);
                CenterHtmlMenu center_votemenu = new CenterHtmlMenu($"<font color='#FFF085' class='fontSize-l'>{image} {Question} {image}</font>", this);
                ChatMenu chat_votemenu = new ChatMenu($"{ChatColors.LightPurple}{Question}", this);
                ConsoleMenu console_votemenu = new ConsoleMenu($"{Question}", this);

                int colorIndex = 0;
                foreach (var item in options)
                {
                    string color = ColorsA[colorIndex % ColorsA.Count];

                    if (Config.MenuType == "wasd")
                    {
                        wasd_votemenu.AddItem($"<font color='{color}'>{item}</font>", (p, o) =>
                        {
                            if (VoteActive)
                            {
                                AddVote(p, item, 2);
                            }
                        });
                    }
                    else if (Config.MenuType == "screen")
                    {
                        s_votemenu.AddItem(item, (p, o) =>
                        {
                            if (VoteActive)
                            {
                                AddVote(p, item, 2);
                            }
                        });
                    }
                    else if (Config.MenuType == "centerhtml")
                    {
                        center_votemenu.AddItem($"<font color='{color}'>{item}</font>", (p, o) =>
                        {
                            if (VoteActive)
                            {
                                AddVote(p, item, 2);
                            }
                        });
                    }
                    else if (Config.MenuType == "chat")
                    {
                        chat_votemenu.AddItem(item, (p, o) =>
                        {
                            if (VoteActive)
                            {
                                AddVote(p, item, 2);
                            }
                        });
                    }
                    else if (Config.MenuType == "console")
                    {
                        console_votemenu.AddItem(item, (p, o) =>
                        {
                            if (VoteActive)
                            {
                                AddVote(p, item, 2);
                            }
                        });
                    }
                    colorIndex++;
                }

                switch (Config.MenuType)
                {
                    case "wasd":
                        wasd_votemenu.Display(caller, 5);
                        break;
                    case "screen":
                        s_votemenu.Display(caller, 5);
                        break;
                    case "centerhtml":
                        center_votemenu.Display(caller, 5);
                        break;
                    case "chat":
                        chat_votemenu.Display(caller, 5);
                        break;
                    case "console":
                        console_votemenu.Display(caller, 5);
                        break;
                    default:
                        wasd_votemenu.Display(caller, 5);
                        break;
                }
            }
            else
            {
                reply(caller, "bv_NotVoted");
            }
        }
    }
}
