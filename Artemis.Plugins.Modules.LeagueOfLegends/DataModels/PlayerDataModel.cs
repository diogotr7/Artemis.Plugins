using Artemis.Core.DataModelExpansions;
using Newtonsoft.Json;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class PlayerDataModel : DataModel
    {
        public AbilityGroupDataModel Abilities { get; set; } = new AbilityGroupDataModel();
        public PlayerStatsDataModel ChampionStats { get; set; } = new PlayerStatsDataModel();
        public InventoryDataModel Inventory { get; set; } = new InventoryDataModel();
        public string SummonerName { get; set; } = "";
        public int Level { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public int CreepScore { get; set; }
        public float Gold { get; set; }
        public float WardScore { get; set; }
        public float RespawnTimer { get; set; }
        public bool IsDead { get; set; }
        public Team Team { get; set; }
        public Champion Champion { get; set; }
        public Position Position { get; set; }
        public SummonerSpell SpellD { get; set; }
        public SummonerSpell SpellF { get; set; }
    }

    public enum Champion
    {
        Unknown = -1,
        None = 0,
        Aatrox,
        Ahri,
        Akali,
        Alistar,
        Amumu,
        Anivia,
        Annie,
        Aphelios,
        Ashe,
        AurelionSol,
        Azir,
        Bard,
        Blitzcrank,
        Brand,
        Braum,
        Caitlyn,
        Camille,
        Cassiopeia,
        Chogath,
        Corki,
        Darius,
        Diana,
        Draven,
        DrMundo,
        Ekko,
        Elise,
        Evelynn,
        Ezreal,
        Fiddlesticks,
        Fiora,
        Fizz,
        Galio,
        Gangplank,
        Garen,
        Gnar,
        Gragas,
        Graves,
        Hecarim,
        Heimerdinger,
        Illaoi,
        Irelia,
        Ivern,
        Janna,
        JarvanIV,
        Jax,
        Jayce,
        Jhin,
        Jinx,
        Kaisa,
        Kalista,
        Karma,
        Karthus,
        Kassadin,
        Katarina,
        Kayle,
        Kayn,
        Kennen,
        Khazix,
        Kindred,
        Kled,
        KogMaw,
        Leblanc,
        LeeSin,
        Leona,
        Lillia,
        Lissandra,
        Lucian,
        Lulu,
        Lux,
        Malphite,
        Malzahar,
        Maokai,
        MasterYi,
        MissFortune,
        Mordekaiser,
        Morgana,
        Nami,
        Nasus,
        Nautilus,
        Neeko,
        Nidalee,
        Nocturne,
        Nunu,
        Olaf,
        Orianna,
        Ornn,
        Pantheon,
        Poppy,
        Pyke,
        Qiyana,
        Quinn,
        Rakan,
        Rammus,
        RekSai,
        Renekton,
        Rengar,
        Riven,
        Rumble,
        Ryze,
        Sejuani,
        Senna,
        Sett,
        Shaco,
        Shen,
        Shyvana,
        Singed,
        Sion,
        Sivir,
        Skarner,
        Sona,
        Soraka,
        Swain,
        Sylas,
        Syndra,
        TahmKench,
        Taliyah,
        Talon,
        Taric,
        Teemo,
        Thresh,
        Tristana,
        Trundle,
        Tryndamere,
        TwistedFate,
        Twitch,
        Udyr,
        Urgot,
        Varus,
        Vayne,
        Veigar,
        Velkoz,
        Vi,
        Viktor,
        Vladimir,
        Volibear,
        Warwick,
        Xayah,
        Xerath,
        XinZhao,
        Wukong,
        Yasuo,
        Yone,
        Yorick,
        Yuumi,
        Zac,
        Zed,
        Ziggs,
        Zilean,
        Zoe,
        Zyra
    }

    public enum Team
    {
        Unknown = -1,
        None = 0,
        Order,
        Chaos
    }

    public enum SummonerSpell
    {
        Unknown = -1,
        None = 0,
        Cleanse,//210
        Exhaust,//210
        Flash,//300
        Ghost,//180
        Heal,//240
        Smite,//oof
        Teleport,//260
        Clarity,//240
        Ignite,//180
        Barrier,//180
        Mark,//80
        Dash//0
    }

    public enum Position
    {
        Unknown = -1,
        None = 0,
        Top,
        Jungle,
        Middle,
        Bot,
        [JsonProperty("UTILITY")]
        Support
    }
}
