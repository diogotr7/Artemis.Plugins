using Artemis.Core.Modules;
using System;
using System.Collections.Generic;

namespace Artemis.Plugins.Modules.Fallout4
{
    public abstract class FalloutCollectionDataModel : DataModel
    {
        protected Dictionary<uint, DynamicChild> Cache { get; }
        protected uint Id { get; }

        protected FalloutCollectionDataModel(uint id)
        {
            Cache = new();
            Id = id;
        }

        internal abstract void Fill(IDictionary<uint, FalloutValue> database);

        protected bool TryUpdateCachedChild<T>(uint key, FalloutValue<T> element)
        {
            if (!Cache.TryGetValue(key, out var child))
            {
                return false;
            }

            if (child is not DynamicChild<T> childOfType)
            {
                throw new Exception();
            }

            childOfType.Value = element.Data;

            return true;
        }

        protected void Yeet<T>()
        {
            
        }
    }
}
