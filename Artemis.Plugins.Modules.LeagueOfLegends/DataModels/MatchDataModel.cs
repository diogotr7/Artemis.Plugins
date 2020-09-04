using Artemis.Core.DataModelExpansions;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class MatchDataModel : DataModel
    {
        public MapTerrain MapTerrain { get; set; }
        public GameMode GameMode { get; set; }
        public string GameModeName { get; set; } = "";
        public bool InGame { get; set; }
        public float GameTime { get; set; }
        public int InfernalDragonsKilled { get; set; }
        public int OceanDragonsKilled { get; set; }
        public int MountainDragonsKilled { get; set; }
        public int CloudDragonsKilled { get; set; }
        public int EarthDragonsKilled { get; set; }
        public int ElderDragonsKilled { get; set; }
        public int DragonsKilled { get; set; }
        public int TurretsKilled { get; set; }
        public int InhibsKilled { get; set; }
        public int BaronsKilled { get; set; }
        public int HeraldsKilled { get; set; }
    }

    //TODO: Find the rest of these
    public enum GameMode
    {
        Unknown = -1,
        None = 0,
        PracticeTool
    }

    public enum MapTerrain
    {
        Unknown = -1,
        None = 0,
        Default,
        Infernal,
        Cloud,
        Mountain,
        Ocean
    }
}
