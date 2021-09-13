using Artemis.Plugins.Modules.Fallout4.Enums;
using System.Collections.Generic;

namespace Artemis.Plugins.Modules.Fallout4.DataModels
{
    internal class DictAccessor<T>
    {
        private readonly Dictionary<uint, (FalloutDataType, object)> cache;
        private readonly uint Id;
        internal T Value => cache.TryGetValue(Id, out (FalloutDataType, object) data)
                                ? (T)data.Item2
                                : default;

        internal DictAccessor(Dictionary<uint, (FalloutDataType, object)> c, uint id)
        {
            cache = c;
            Id = id;
        }
    }
}