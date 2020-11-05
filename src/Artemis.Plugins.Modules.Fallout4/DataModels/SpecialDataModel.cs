namespace Artemis.Plugins.Modules.Fallout4.DataModels
{
    public class SpecialDataModel
    {
        internal DictAccessor<int> strength;
        internal DictAccessor<int> perception;
        internal DictAccessor<int> endurance;
        internal DictAccessor<int> charisma;
        internal DictAccessor<int> intelligence;
        internal DictAccessor<int> agility;
        internal DictAccessor<int> luck;

        public int Strength => strength?.Value ?? default;
        public int Perception => perception?.Value ?? default;
        public int Endurance => endurance?.Value ?? default;
        public int Charisma => charisma?.Value ?? default;
        public int Intelligence => intelligence?.Value ?? default;
        public int Agility => agility?.Value ?? default;
        public int Luck => luck?.Value ?? default;
    }
}