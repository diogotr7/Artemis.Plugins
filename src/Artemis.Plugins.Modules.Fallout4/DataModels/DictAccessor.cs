using Artemis.Plugins.Modules.Fallout4.Enums;
using System.Collections.Generic;

namespace Artemis.Plugins.Modules.Fallout4.DataModels
{
    internal class DictAccessor<T>
    {
        private readonly Dictionary<uint, (DataType, object)> cache;
        private readonly uint Id;
        internal T Value => cache.TryGetValue(Id, out (DataType, object) data)
                                ? (T)data.Item2
                                : default;

        internal DictAccessor(Dictionary<uint, (DataType, object)> c, uint id)
        {
            cache = c;
            Id = id;
        }
    }
}