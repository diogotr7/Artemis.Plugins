using Artemis.Core.Modules;
using Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels;
using System;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class ItemSlotDataModel : DataModel
    {
        public string Name { get; set; }
        public bool HasItem => !string.IsNullOrWhiteSpace(Name);
        public bool CanUse { get; set; }
        public bool Consumable { get; set; }
        public int Count { get; set; }

        public void Apply(Item[] items, int index)
        {
            var item = Array.Find(items, i => i.Slot == index);
            if (item == null)
            {
                Name = "";
                CanUse = false;
                Consumable = false;
                Count = 0;
                return;
            }

            Name = item.DisplayName;
            CanUse = item.CanUse;
            Consumable = item.Consumable;
            Count = item.Count;
        }
    }
}
