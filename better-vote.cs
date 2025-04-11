using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CounterStrikeSharp.API.Core.Listeners;

namespace better_vote
{
    public partial class better_vote : BasePlugin, IPluginConfig<VoteConfig>
    {
        public override string ModuleName => "Better Vote System";
        public override string ModuleVersion => "1.0.0";
        public override string ModuleAuthor => "varkit";
        public VoteConfig Config { get; set; }
        public string prefix{ get; set; }

        public override void Load(bool hotReload)
        {
            Console.WriteLine(" ");
            Console.WriteLine($">> {ModuleName} <<");
            Console.WriteLine($">> Version: {ModuleVersion} <<");
            Console.WriteLine(" ");

            RegisterListener<Listeners.OnTick>(OnTick);
            RegisterListener<Listeners.OnMapStart>(OnMapStart);
        }

        public void OnConfigParsed(VoteConfig config)
        {
            Config = config;
            prefix = config.Prefix.ReplaceColorTags();
            VoteDuration = config.VoteDuration;
            RTV_VoteDuration = config.RTV_VoteDuration;
        }

        public void OnTick()
        {
            if (VoteActive)
            {
                var image = $"<img src='https://raw.githubusercontent.com/vulikit/better-vote/refs/heads/main/resources/votebox.png'>";
                var title = $"<font color='#FFF085' class='fontSize-l'>{image} {Question} {image}</font><br>";

                string optionsText = "";
                var sortedOptions = options.Select(option => new
                {
                    Option = option,
                    VoteCount = voteData.ContainsKey(option) ? voteData[option] : 0,
                    Color = ColorsA[options.IndexOf(option) % ColorsA.Count]
                })
                .OrderByDescending(x => x.VoteCount)
                .ToList();

                foreach (var item in sortedOptions)
                {
                    optionsText += $"<font color='{item.Color}'>{item.Option}</font> <font color='#E69DB8'>|</font> <font color='white'>{item.VoteCount}</font><br>";
                }

                PrintToCenterHtmlAll($" {title} {optionsText}");
            }
        }
    }
}
