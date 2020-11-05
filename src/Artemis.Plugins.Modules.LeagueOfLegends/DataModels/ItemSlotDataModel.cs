using Artemis.Plugins.Modules.LeagueOfLegends.GameData;
using System;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class ItemSlotDataModel
    {
        private readonly Func<AllPlayer> allPlayer;
        private readonly int itemIndex;
        private Item Item => allPlayer().Items?.Length >= itemIndex ? allPlayer().Items[itemIndex] : default;

        public ItemSlotDataModel(Func<AllPlayer> accessor, int index)
        {
            allPlayer = accessor;
            itemIndex = index;
        }

        public string Name => Item.DisplayName;
        public bool HasItem => !string.IsNullOrWhiteSpace(Name);
        public bool CanUse => Item.CanUse;
        public bool Consumable => Item.Consumable;
        public int Count => Item.Count;
    }
}
