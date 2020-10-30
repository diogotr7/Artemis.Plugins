namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class AbilityGroupDataModel : ChildDataModel
    {
        public AbilityGroupDataModel(LeagueOfLegendsDataModel root) : base(root)
        {
            Q = new AbilityDataModel(() => RootGameData.ActivePlayer.Abilities.Q);
            E = new AbilityDataModel(() => RootGameData.ActivePlayer.Abilities.W);
            W = new AbilityDataModel(() => RootGameData.ActivePlayer.Abilities.E);
            R = new AbilityDataModel(() => RootGameData.ActivePlayer.Abilities.R);
        }

        public AbilityDataModel Q { get; set; }
        public AbilityDataModel W { get; set; }
        public AbilityDataModel E { get; set; }
        public AbilityDataModel R { get; set; }
    }
}
