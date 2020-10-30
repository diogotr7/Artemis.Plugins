using Artemis.Core.DataModelExpansions;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class MatchDataModel : DataModel
    {
        public MapTerrain MapTerrain { get; set; }
        public GameMode GameMode { get; set; }
        public bool InGame { get; set; }
        public float GameTime { get; set; }

        internal void Reset()
        {
            MapTerrain = MapTerrain.None;
            GameMode = GameMode.None;
            InGame = false;
            GameTime = -1;
        }
    }
}
