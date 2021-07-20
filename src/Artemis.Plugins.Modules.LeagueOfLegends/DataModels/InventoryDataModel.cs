using Artemis.Core.Modules;
using Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels;
using System;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class InventoryDataModel : DataModel
    {
        public ItemSlotDataModel Slot1 { get; } = new();
        public ItemSlotDataModel Slot2 { get; }= new();
        public ItemSlotDataModel Slot3 { get; }= new();
        public ItemSlotDataModel Slot4 { get; }= new();
        public ItemSlotDataModel Slot5 { get; }= new();
        public ItemSlotDataModel Slot6 { get; } = new();
        public ItemSlotDataModel Trinket { get; } = new();

        public int ItemCount { get; set; }

        public void Apply(Item[] items)
        {
            Slot1.Apply(items, 0);
            Slot2.Apply(items, 1);
            Slot3.Apply(items, 2);
            Slot4.Apply(items, 3);
            Slot5.Apply(items, 4);
            Slot6.Apply(items, 5);
            Trinket.Apply(items, 6);
            ItemCount = items.Length;
        }
    }
}
