using Artemis.Core.DataModelExpansions;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums;

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
        public int ElderDragonsKilled { get; set; }
        public int DragonsKilled { get; set; }
        public int TurretsKilled { get; set; }
        public int InhibsKilled { get; set; }
        public int BaronsKilled { get; set; }
        public int HeraldsKilled { get; set; }

        internal void Reset()
        {
            MapTerrain = MapTerrain.None;
            GameMode = GameMode.None;
            GameModeName = "";
            InGame = false;
            GameTime = -1;
            InfernalDragonsKilled = -1;
            OceanDragonsKilled = -1;
            MountainDragonsKilled = -1;
            CloudDragonsKilled = -1;
            ElderDragonsKilled = -1;
            DragonsKilled = -1;
            TurretsKilled = -1;
            InhibsKilled = -1;
            BaronsKilled = -1;
            HeraldsKilled = -1;
        }
    }
}
