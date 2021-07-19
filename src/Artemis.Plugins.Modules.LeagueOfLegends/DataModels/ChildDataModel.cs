using Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public abstract class ChildDataModel
    {
        protected ChildDataModel(LeagueOfLegendsDataModel root) => DataModelRoot = root;

        protected LeagueOfLegendsDataModel DataModelRoot { get; }

        private protected RootGameData RootGameData => DataModelRoot.RootGameData;
    }
}
