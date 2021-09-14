using Artemis.Core.Modules;
using System;
using System.Collections.Generic;

namespace Artemis.Plugins.Modules.Fallout4
{
    public class FalloutMapDataModel : FalloutCollectionDataModel
    {
        public FalloutMapDataModel() : this(0) { }
        internal FalloutMapDataModel(uint id) : base(id) { }

        internal override void Fill(IDictionary<uint, FalloutValue> database)
        {
            if (database[Id] is not MapFalloutElement parentMap)
                throw new Exception();

            foreach ((var key, var name) in parentMap.Data)
            {
                switch (database[key])
                {
                    case BoolFalloutElement boolElement:
                        if (!TryUpdateCachedChild(key, boolElement))
                        {
                            Cache[key] = AddDynamicChild(name, boolElement.Data);
                        }
                        break;
                    case SByteFalloutElement sbyteElement:
                        if (!TryUpdateCachedChild(key, sbyteElement))
                        {
                            Cache[key] = AddDynamicChild(name, sbyteElement.Data);
                        }
                        break;
                    case ByteFalloutElement byteElement:
                        if (!TryUpdateCachedChild(key, byteElement))
                        {
                            Cache[key] = AddDynamicChild(name, byteElement.Data);
                        }
                        break;
                    case IntFalloutElement intElement:
                        if (!TryUpdateCachedChild(key, intElement))
                        {
                            Cache[key] = AddDynamicChild(name, intElement.Data);
                        }
                        break;
                    case UIntFalloutElement uintElement:
                        if (!TryUpdateCachedChild(key, uintElement))
                        {
                            Cache[key] = AddDynamicChild(name, uintElement.Data);
                        }
                        break;
                    case FloatFalloutElement floatElement:
                        if (!TryUpdateCachedChild(key, floatElement))
                        {
                            Cache[key] = AddDynamicChild(name, floatElement.Data);
                        }
                        break;
                    case StringFalloutElement stringElement:
                        if (!TryUpdateCachedChild(key, stringElement))
                        {
                            Cache[key] = AddDynamicChild(name, stringElement.Data);
                        }
                        break;
                    case ArrayFalloutElement:
                        if (!Cache.TryGetValue(key, out var arrayDynamicChild))
                        {
                            arrayDynamicChild = AddDynamicChild(name, new FalloutArrayDataModel(key));
                            Cache[key] = arrayDynamicChild;
                        }
                        (arrayDynamicChild as DynamicChild<FalloutArrayDataModel>).Value.Fill(database);
                        break;
                    case MapFalloutElement:
                        if (!Cache.TryGetValue(key, out var mapDynamicChild))
                        {
                            mapDynamicChild = AddDynamicChild(name, new FalloutMapDataModel(key));
                            Cache[key] = mapDynamicChild;
                        }
                        (mapDynamicChild as DynamicChild<FalloutMapDataModel>).Value.Fill(database);
                        break;
                }
            }
        }
    }
}
