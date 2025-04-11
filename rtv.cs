using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
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
        public int rtvCount = 0;
        public bool IsRTVvoteActive = false;
        public List<int> RTVPlayers = new List<int>();
        public float RTV_VoteDuration = 15;
        public List<string> NominatedMaps = new List<string>();
        public bool IsExtendVoteActive = false;
        public int extendVoteCount = 0;
        public List<int> ExtendVotedPlayers = new List<int>();
        public int ExtendedRoundCount = 0;
        public int ExtendStartRound = 0;

        [ConsoleCommand("css_rtv")]
        public void Command_RTV(CCSPlayerController? caller, CommandInfo command)
        {
            if (caller == null || !caller.IsValid || caller.IsBot || caller.IsHLTV)
                return;

            if (IsRTVvoteActive)
            {
                reply(caller, "bv_RTV_AlreadyRTV");
                return;
            }

            if (IsExtendVoteActive)
            {
                reply(caller, "bv_Extend_AlreadyActive");
                return;
            }

            if (RTVPlayers.Contains(caller.Slot))
            {
                reply(caller, "bv_RTV_AlreadyVoted");
                return;
            }

            int playerCount = GetPlayerCount();
            if (playerCount < Config.RTV_MinimumPlayerCount)
            {
                reply(caller, "bv_RTV_NeedPlayer", Config.RTV_MinimumPlayerCount);
                return;
            }

            int requiredVotes = (playerCount / 2) + 1;
            rtvCount++;
            RTVPlayers.Add(caller.Slot);
            broadcast("bv_RTV_PlayerVotedBroadcast", caller.PlayerName, rtvCount, requiredVotes);
        }

        [ConsoleCommand("css_nominate")]
        public void Command_Nominate(CCSPlayerController? caller, CommandInfo command)
        {
            if (caller == null || !caller.IsValid || caller.IsBot || caller.IsHLTV)
                return;

            if (IsRTVvoteActive || IsExtendVoteActive)
            {
                reply(caller, "bv_Nominate_VoteActive");
                return;
            }

            var availableMaps = Config.RTV_Maps
                .Where(kvp => kvp.Key != Server.MapName)
                .Select(kvp => kvp.Key)
                .ToList();

            if (!availableMaps.Any())
            {
                reply(caller, "bv_Nominate_NoMaps");
                return;
            }

            WasdMenu menu = new WasdMenu(Localizer["bv_NominateMenuTitle"], this);
            foreach (var map in availableMaps)
            {
                menu.AddItem(map, (player, option) =>
                {
                    if (NominatedMaps.Contains(map))
                    {
                        reply(player, "bv_Nominate_AlreadyNominated", map);
                        return;
                    }

                    if (NominatedMaps.Count >= Config.RTV_MaxMapOption)
                    {
                        reply(player, "bv_Nominate_MaxNominations");
                        return;
                    }

                    NominatedMaps.Add(map);
                    broadcast("bv_Nominate_Success", player.PlayerName, map);
                });
            }

            menu.Display(caller, 20);
        }

        [ConsoleCommand("css_extend")]
        public void Command_Extend(CCSPlayerController? caller, CommandInfo command)
        {
            if (caller == null || !caller.IsValid || caller.IsBot || caller.IsHLTV)
                return;

            if (IsRTVvoteActive)
            {
                reply(caller, "bv_RTV_AlreadyRTV");
                return;
            }

            if (IsExtendVoteActive || VoteActive)
            {
                reply(caller, "bv_Extend_AlreadyActive");
                return;
            }

            int playerCount = GetPlayerCount();
            if (playerCount < Config.RTV_MinimumPlayerCount)
            {
                reply(caller, "bv_Extend_NeedPlayer", Config.RTV_MinimumPlayerCount);
                return;
            }

            StartExtendVote();
        }

        public void StartExtendVote()
        {
            IsExtendVoteActive = true;
            extendVoteCount = 0;
            ExtendVotedPlayers.Clear();
            ExtendStartRound = GetRoundCount();

            VoteActive = true;
            options.Clear();
            voteData.Clear();
            VotedPlayers.Clear();
            Question = Localizer["bv_ExtendMenuTitle"];
            options.Add(Localizer["bv_Yes"]);
            options.Add(Localizer["bv_No"]);

            StartVoting();
            AddTimer(RTV_VoteDuration, () => EndExtendVote());
        }

        public void EndExtendVote()
        {
            if (!IsExtendVoteActive)
                return;

            IsExtendVoteActive = false;
            VoteActive = false;
            int playerCount = GetPlayerCount();
            int requiredVotes = (playerCount / 2) + 1;

            int yesVotes = voteData.ContainsKey(Localizer["bv_Yes"]) ? voteData[Localizer["bv_Yes"]] : 0;
            if (yesVotes >= requiredVotes)
            {
                ExtendedRoundCount = ExtendStartRound + Config.RTV_MinimumRound - GetRoundCount();
                broadcast("bv_Extend_Success", Config.RTV_MinimumRound);
                rtvCount = 0;
                RTVPlayers.Clear();
            }
            else
            {
                broadcast("bv_Extend_Failed");
            }

            ExtendVotedPlayers.Clear();
            extendVoteCount = 0;
        }

        public void StartRTV()
        {
            IsRTVvoteActive = true;
            options.Clear();
            voteData.Clear();
            VotedPlayers.Clear();
            Question = Config.RTV_Title.ReplaceColorTags();

            foreach (var map in NominatedMaps)
            {
                if (!options.Contains(map))
                {
                    options.Add(map);
                }
            }

            var randomMaps = GetRandomMaps();
            foreach (var item in randomMaps)
            {
                if (!options.Contains(item) && options.Count < Config.RTV_MaxMapOption)
                {
                    options.Add(item);
                }
            }
            StartVoting();
        }

        public int GetPlayerCount()
        {
            return Utilities.GetPlayers().Where(p => p is { IsHLTV: false, IsBot: false, IsValid: true }).ToList().Count();
        }

        public int GetRoundCount()
        {
            var gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault();
            if (gameRules != null && gameRules.GameRules != null)
            {
                return gameRules.GameRules.TotalRoundsPlayed;
            }
            return 0;
        }

        public List<string> GetRandomMaps()
        {
            var availableMaps = Config.RTV_Maps
                .Where(kvp => kvp.Key != Server.MapName && !NominatedMaps.Contains(kvp.Key))
                .Select(kvp => kvp.Key)
                .ToList();

            Random random = new Random();
            int maxOptions = Math.Min(Config.RTV_MaxMapOption - NominatedMaps.Count, availableMaps.Count);
            List<string> selectedMaps = new List<string>();

            for (int i = 0; i < maxOptions; i++)
            {
                if (availableMaps.Count == 0) break;

                int randomIndex = random.Next(0, availableMaps.Count);
                selectedMaps.Add(availableMaps[randomIndex]);
                availableMaps.RemoveAt(randomIndex);
            }

            return selectedMaps;
        }

        public void ChangeMap(string selectedMap)
        {
            if (Config.RTV_Maps.TryGetValue(selectedMap, out var mapInfo))
            {
                if (mapInfo.IsWorkshop)
                {
                    Server.ExecuteCommand($"host_workshop_map {mapInfo.WorkshopId}");
                }
                else
                {
                    Server.ExecuteCommand($"changelevel {selectedMap}");
                }
            }
            else
            {
                broadcast("bv_Map_NotFound", selectedMap);
            }
        }
    }
}