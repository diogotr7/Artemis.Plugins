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

        public ItemSlotDataModel(_Item item)
        {
            CanUse = item.canUse;
            Consumable = item.consumable;
            Count = item.count;
            Name = item.displayName;
        }
    }
}
