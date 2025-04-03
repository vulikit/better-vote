using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using CS2MenuManager.API.Class;
using MenunManager2 = CounterStrikeSharp.API.Modules.Menu.MenuManager;
using CS2MenuManager.API.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using CounterStrikeSharp.API.Core.Translations;
using System.Numerics;
using CS2MenuManager.API.Interface;

namespace better_vote
{
    public partial class better_vote
    {
        public bool VoteActive = false;
        public List<string> options = new List<string>();
        public string Question = "";
        public float VoteDuration { get; set; }
        public Dictionary<string, int> voteData = new Dictionary<string, int>();
        public Dictionary<int, string> VotedPlayers = new Dictionary<int, string>();
        public List<string> ColorsA = new List<string>()
        {
            "#FF6B6B",
            "#FFA07A",
            "#FFD700",
            "#32CD32",
            "#1E90FF",
            "#9370DB",
            "#FF69B4"
        };

        public void StartVoting()
        {
            foreach (var player in Utilities.GetPlayers().Where(p => p is { IsHLTV: false, IsBot: false, IsValid: true }))
            {
                MenuManager.CloseActiveMenu(player);
                MenunManager2.CloseActiveMenu(player);
            }
            VoteActive = true;
            AddTimer(VoteDuration, () =>
            {
                foreach (var player in Utilities.GetPlayers().Where(p => p is { IsHLTV: false, IsBot: false, IsValid: true }))
                {
                    MenuManager.CloseActiveMenu(player);
                    MenunManager2.CloseActiveMenu(player);
                }
                if (voteData.Any())
                {
                    var winner = voteData.OrderByDescending(x => x.Value).First();
                    int maxVotes = winner.Value;
                    string winningOption = winner.Key;

                    var winners = voteData.Where(x => x.Value == maxVotes).Select(x => x.Key).ToList();
                    if (winners.Count > 1)
                    {
                        string tiedOptions = string.Join(", ", winners);
                        broadcast("bv_VoteTie", tiedOptions, maxVotes);
                    }
                    else
                    {
                        broadcast("bv_VoteWinner", winningOption, maxVotes);
                    }
                }
                else
                {
                    broadcast("bv_NoVotesCast");
                }

                options.Clear();
                Question = "";
                voteData.Clear();
                VotedPlayers.Clear();
                broadcast("bv_VoteStopped");
                VoteActive = false;
            });
            var image = $"<img src='https://raw.githubusercontent.com/vulikit/better-vote/refs/heads/main/resources/votebox.png'>";
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
                            AddVote(p, item, 1);
                        }
                    });
                }
                else if (Config.MenuType == "screen")
                {
                    s_votemenu.AddItem(item, (p, o) =>
                    {
                        if (VoteActive)
                        {
                            AddVote(p, item, 1);
                        }
                    });
                }
                else if (Config.MenuType == "centerhtml")
                {
                    center_votemenu.AddItem($"<font color='{color}'>{item}</font>", (p, o) =>
                    {
                        if (VoteActive)
                        {
                            AddVote(p, item, 1);
                        }
                    });
                }
                else if (Config.MenuType == "chat")
                {
                    chat_votemenu.AddItem(item, (p, o) =>
                    {
                        if (VoteActive)
                        {
                            AddVote(p, item, 1);
                        }
                    });
                }
                else if (Config.MenuType == "console")
                {
                    console_votemenu.AddItem(item, (p, o) =>
                    {
                        if (VoteActive)
                        {
                            AddVote(p, item, 1);
                        }
                    });
                }
                colorIndex++;
            }

            switch (Config.MenuType)
            {
                case "wasd":
                    wasd_votemenu.DisplayToAll((int)VoteDuration);
                    break;
                case "screen":
                    s_votemenu.DisplayToAll((int)VoteDuration);
                    break;
                case "centerhtml":
                    center_votemenu.DisplayToAll((int)VoteDuration);
                    break;
                case "chat":
                    chat_votemenu.DisplayToAll((int)VoteDuration);
                    break;
                case "console":
                    console_votemenu.DisplayToAll((int)VoteDuration);
                    break;
                default:
                    wasd_votemenu.DisplayToAll((int)VoteDuration);
                    break;
            }
        }

        public void AddVote(CCSPlayerController player, string opt, int votenum)
        {
            if (VotedPlayers.ContainsKey(player.Slot))
            {
                if (votenum == 2)
                {
                    string previousVote = VotedPlayers[player.Slot];
                    if (voteData.ContainsKey(previousVote))
                    {
                        voteData[previousVote]--;
                        if (voteData[previousVote] <= 0)
                        {
                            voteData.Remove(previousVote);
                        }
                    }
                    VotedPlayers.Remove(player.Slot);
                }
                else
                {
                    reply(player, "bv_AlreadyVoted", VotedPlayers[player.Slot]);
                    return;
                }
            }

            if (votenum == 1)
            {
                reply(player, "bv_VotedFor", opt);
                if (Config.VotedMessage)
                {
                    broadcast("bv_PlayerVotedFor", player.PlayerName, opt);
                }
            }
            else if (votenum == 2)
            {
                reply(player, "bv_VotedAgainFor", opt);
                if (Config.VotedMessage)
                {
                    broadcast("bv_PlayerAgainVotedFor", player.PlayerName, opt);
                }
            }

            if (voteData.ContainsKey(opt))
            {
                voteData[opt]++;
            }
            else
            {
                voteData[opt] = 1;
            }
            VotedPlayers.Add(player.Slot, opt);
        }


        public void reply(CCSPlayerController player, string message, params object[] args)
        {
            string localizedMessage = Localizer.ForPlayer(player, message, args);
            player.PrintToChat(prefix + localizedMessage.ReplaceColorTags());
        }

        public void broadcast(string message, params object[] args)
        {
            foreach (var player in Utilities.GetPlayers().Where(p => p is { IsHLTV: false, IsBot: false, IsValid: true }))
            {
                string localizedMessage = Localizer.ForPlayer(player, message, args);
                player.PrintToChat(prefix + localizedMessage.ReplaceColorTags());
            }
        }

        public void PrintToCenterHtmlAll(string message)
        {
            foreach (var player in Utilities.GetPlayers().Where(p => p is { IsHLTV: false, IsBot: false, IsValid: true }))
            {
                player.PrintToCenterHtml(message);
            }
        }

        public bool PlayerHasPermission(CCSPlayerController player, List<string> perm)
        {
            if (perm.Any(p => AdminManager.PlayerHasPermissions(player, p))) return true;
            reply(player, $" {ChatColors.Red}No permission.");
            return false;
        }

        public bool CheckActiveVoting(CCSPlayerController player)
        {
            if(VoteActive)
            {
                return true;
            }
            return false;
        }
    }
}
