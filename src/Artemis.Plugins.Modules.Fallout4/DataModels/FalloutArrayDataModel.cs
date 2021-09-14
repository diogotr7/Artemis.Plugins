using Artemis.Core.Modules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Artemis.Plugins.Modules.Fallout4
{
    public class FalloutArrayDataModel : FalloutCollectionDataModel
    {
        internal FalloutArrayDataModel(uint id) : base(id) { }

        internal override void Fill(IDictionary<uint, FalloutValue> database)
        {
            if (database[Id] is not ArrayFalloutElement parentArray)
                throw new Exception();
            int i = 0;
            foreach (var item in parentArray.Data)
            {
                switch (database[item])
                {
                    case BoolFalloutElement boolElement:
                        if (!TryUpdateCachedChild(item, boolElement))
                        {
                            Cache[item] = AddDynamicChild((i++).ToString(), boolElement.Data);
                        }
                        break;
                    case SByteFalloutElement sbyteElement:
                        if (!TryUpdateCachedChild(item, sbyteElement))
                        {
                            Cache[item] = AddDynamicChild((i++).ToString(), sbyteElement.Data);
                        }
                        break;
                    case ByteFalloutElement byteElement:
                        if (!TryUpdateCachedChild(item, byteElement))
                        {
                            Cache[item] = AddDynamicChild((i++).ToString(), byteElement.Data);
                        }
                        break;
                    case IntFalloutElement intElement:
                        if (!TryUpdateCachedChild(item, intElement))
                        {
                            Cache[item] = AddDynamicChild((i++).ToString(), intElement.Data);
                        }
                        break;
                    case UIntFalloutElement uintElement:
                        if (!TryUpdateCachedChild(item, uintElement))
                        {
                            Cache[item] = AddDynamicChild((i++).ToString(), uintElement.Data);
                        }
                        break;
                    case FloatFalloutElement floatElement:
                        if (!TryUpdateCachedChild(item, floatElement))
                        {
                            Cache[item] = AddDynamicChild((i++).ToString(), floatElement.Data);
                        }
                        break;
                    case StringFalloutElement stringElement:
                        if (!TryUpdateCachedChild(item, stringElement))
                        {
                            Cache[item] = AddDynamicChild((i++).ToString(), stringElement.Data);
                        }
                        break;
                    case ArrayFalloutElement:
                        if (!Cache.TryGetValue(item, out var arrayDynamicChild))
                        {
                            arrayDynamicChild = AddDynamicChild((i++).ToString(), new FalloutArrayDataModel(item));
                            Cache[item] = arrayDynamicChild;
                        }
                        (arrayDynamicChild as DynamicChild<FalloutArrayDataModel>).Value.Fill(database);
                        break;
                    case MapFalloutElement mapElement:
                        if (!Cache.TryGetValue(item, out var mapDynamicChild))
                        {
                            //here we have an array with maps inside.
                            //It's useful if we can name each Map inside the array
                            //something useful. Sometimes the array itself has a 
                            //Name property, let's check that.
                            var nameChildElement = mapElement.Data.FirstOrDefault(kvp => kvp.Value == "Text" || kvp.Value == "Name");
                            if (!string.IsNullOrWhiteSpace(nameChildElement.Value)
                                && nameChildElement.Key != 0
                                && database.TryGetValue(nameChildElement.Key, out var asd)
                                && asd is StringFalloutElement stringNameElement
                                && !string.IsNullOrWhiteSpace(stringNameElement.Data)
                                && !DynamicChildren.TryGetValue(stringNameElement.Data.Replace('.', '_'), out var _))
                            {
                                mapDynamicChild = AddDynamicChild(stringNameElement.Data.Replace('.', '_'), new FalloutMapDataModel(item));
                            }
                            else
                            {
                                mapDynamicChild = AddDynamicChild((i++).ToString(), new FalloutMapDataModel(item));
                            }

                            Cache[item] = mapDynamicChild;
                        }
                        (mapDynamicChild as DynamicChild<FalloutMapDataModel>).Value.Fill(database);
                        break;
                }
            }
        }
    }
}
