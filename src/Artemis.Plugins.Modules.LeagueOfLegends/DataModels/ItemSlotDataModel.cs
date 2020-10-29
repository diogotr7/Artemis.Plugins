using Artemis.Core.DataModelExpansions;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class ItemSlotDataModel : DataModel
    {
        public string Name { get; set; } = "";
        public bool HasItem => !string.IsNullOrWhiteSpace(Name);
        public bool CanUse { get; set; }
        public bool Consumable { get; set; }
        public int Count { get; set; }

        public ItemSlotDataModel()
        {
            CanUse = false;
            Consumable = false;
            Count = 0;
            Name = null;
        }

        internal ItemSlotDataModel(Item item)
        {
            CanUse = item.CanUse;
            Consumable = item.Consumable;
            Count = item.Count;
            Name = item.DisplayName;
        }

        internal void Reset()
        {
            Name = "";
            CanUse = false;
            Consumable = false;
            Count = -1;
        }
    }
}
