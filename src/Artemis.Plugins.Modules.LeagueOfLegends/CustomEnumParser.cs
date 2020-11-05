using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Artemis.Plugins.Modules.LeagueOfLegends
{
    //adapted from https://stackoverflow.com/questions/30526757
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    internal class NameAttribute : Attribute
    {
        public readonly string[] Names;

        public NameAttribute(params string[] names)
        {
            if (names?.Any(x => x == null) ?? false)
            {
                throw new ArgumentNullException(nameof(names));
            }

            Names = names.Distinct().ToArray();
        }
    }
    internal static class ParseEnum<TEnum> where TEnum : struct, Enum
    {
        private static readonly Dictionary<string, TEnum> Values = new Dictionary<string, TEnum>();

        static ParseEnum()
        {
            //add custom values first.
            //defined with NameAttribute on each enum value
            Values = typeof(TEnum)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(a => new { NameAtt = a.GetCustomAttribute<NameAttribute>(), EnumValue = (TEnum)a.GetValue(null) })
                .Where(a => a.NameAtt != null)
                .SelectMany(field => field.NameAtt.Names, (field, name) => new { Key = name, Value = field.EnumValue })
                .ToDictionary(a => a.Key, a => a.Value);

            //then we cache every possible enum value so we don't have to call TryParse
            foreach (TEnum e in Enum.GetValues(typeof(TEnum)))
                Values.Add(Enum.GetName(typeof(TEnum), e), e);
        }

        internal static TEnum TryParseOr(string value, TEnum defaultValue)
        {
            //this should be None for all enums in this plugin
            if (string.IsNullOrWhiteSpace(value))
                return default;

            if (Values.TryGetValue(value, out TEnum result))
                return result;

            return defaultValue;
        }
    }
}