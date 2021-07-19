using Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels;
using System;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class InventoryDataModel
    {
        private readonly Func<AllPlayer> _allPlayer;
        public InventoryDataModel(Func<AllPlayer> allPlayer)
        {
            _allPlayer = allPlayer;
            Slot1 = new ItemSlotDataModel(allPlayer, 0);
            Slot2 = new ItemSlotDataModel(allPlayer, 1);
            Slot3 = new ItemSlotDataModel(allPlayer, 2);
            Slot4 = new ItemSlotDataModel(allPlayer, 3);
            Slot5 = new ItemSlotDataModel(allPlayer, 4);
            Slot6 = new ItemSlotDataModel(allPlayer, 5);
            Trinket = new ItemSlotDataModel(allPlayer, 6);
        }

        public ItemSlotDataModel Slot1 { get; }
        public ItemSlotDataModel Slot2 { get; }
        public ItemSlotDataModel Slot3 { get; }
        public ItemSlotDataModel Slot4 { get; }
        public ItemSlotDataModel Slot5 { get; }
        public ItemSlotDataModel Slot6 { get; }
        public ItemSlotDataModel Trinket { get; }

        public int ItemCount => _allPlayer().Items?.Length ?? 0;
    }
}
