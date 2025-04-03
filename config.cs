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

        [JsonPropertyName("MenuType")]
        public string MenuType { get; set; } = "wasd";

        [JsonPropertyName("VoteDuration")]
        public float VoteDuration { get; set; } = 20;
    }
}
