using CounterStrikeSharp.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace better_vote
{
    public class VoteConfig : BasePluginConfig
    {
        [JsonPropertyName("Prefix")]
        public string Prefix { get; set; } = "{blue}⌈ Better Vote ⌋";

        [JsonPropertyName("VotePermission")]
        public List<string> VotePermission { get; set; } = new List<string>() {
            "@css/vote"
        };

        [JsonPropertyName("VotedMessage")]
        public bool VotedMessage { get; set; } = true;

        [JsonPropertyName("EndVotingOnRoundEnd")]
        public bool EndVotingOnRoundEnd { get; set; } = true;

        [JsonPropertyName("MenuType")]
        public string MenuType { get; set; } = "wasd";

        [JsonPropertyName("VoteDuration")]
        public float VoteDuration { get; set; } = 20;

        //RTV feature
        [JsonPropertyName("IsRTVactive")]
        public bool IsRTVactive { get; set; } = true;

        [JsonPropertyName("RTV_MinimumRound")]
        public int RTV_MinimumRound { get; set; } = 5;

        [JsonPropertyName("RTV_MinimumPlayerCount")]
        public int RTV_MinimumPlayerCount { get; set; } = 1;

        [JsonPropertyName("RTV_VoteDuration")]
        public float RTV_VoteDuration { get; set; } = 15;

        [JsonPropertyName("RTV_Title")]
        public string RTV_Title { get; set; } = "Choose Map";

        [JsonPropertyName("RTV_MaxMapOption")]
        public int RTV_MaxMapOption { get; set; } = 5;

        [JsonPropertyName("RTV_Maps")]
        public Dictionary<string, MapInfo> RTV_Maps { get; set; } = new Dictionary<string, MapInfo>
            {
                { "de_dust2", new MapInfo(false, "") },
                { "de_mirage", new MapInfo(false, "") },
                { "de_inferno", new MapInfo(false, "") },
                { "de_nuke", new MapInfo(false, "") },
                { "de_overpass", new MapInfo(false, "") },
                { "de_vertigo", new MapInfo(false, "") },
                { "de_ancient", new MapInfo(false, "") }
            };

        public class MapInfo
        {
            [JsonPropertyName("IsWorkshop")]
            public bool IsWorkshop { get; set; }

            [JsonPropertyName("WorkshopId")]
            public string WorkshopId { get; set; } = string.Empty;

            public MapInfo(bool isWorkshop, string workshopId)
            {
                IsWorkshop = isWorkshop;
                WorkshopId = workshopId;
            }
            public MapInfo() { }
        }
    }
}
