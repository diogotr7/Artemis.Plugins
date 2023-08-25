using System;
using System.Collections.Generic;

namespace Artemis.Plugins.LayerBrushes.Chroma.Utils;

public static class EnumToString<T> where T : Enum
{
    private static readonly Dictionary<T, string> _cache = new();
    
    public static string Get(T value)
    {
        if (_cache.TryGetValue(value, out string? result))
            return result;

        result = value.ToString();
        _cache[value] = result;
        return result;
    }
    
}