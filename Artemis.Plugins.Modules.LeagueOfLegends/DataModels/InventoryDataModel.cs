using Artemis.Core.DataModelExpansions;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class InventoryDataModel : DataModel
    {
        public ItemSlotDataModel Slot1 { get; set; } = new ItemSlotDataModel();
        public ItemSlotDataModel Slot2 { get; set; } = new ItemSlotDataModel();
        public ItemSlotDataModel Slot3 { get; set; } = new ItemSlotDataModel();
        public ItemSlotDataModel Slot4 { get; set; } = new ItemSlotDataModel();
        public ItemSlotDataModel Slot5 { get; set; } = new ItemSlotDataModel();
        public ItemSlotDataModel Slot6 { get; set; } = new ItemSlotDataModel();
        public ItemSlotDataModel Trinket { get; set; } = new ItemSlotDataModel();
    }
}
