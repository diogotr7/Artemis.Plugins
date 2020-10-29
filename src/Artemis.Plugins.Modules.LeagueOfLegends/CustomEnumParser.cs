using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums;
using Artemis.Plugins.Modules.LeagueOfLegends.LeagueOfLegendsConfigurationDialog;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

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
        internal static readonly Dictionary<string, TEnum> Values = new Dictionary<string, TEnum>();

        static ParseEnum()
        {
            Values = typeof(TEnum)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(a => new { NameAtt = a.GetCustomAttribute<NameAttribute>(), EnumValue = (TEnum)a.GetValue(null) })
                .Where(a => a.NameAtt != null)
                .SelectMany(field => field.NameAtt.Names, (field, name) => new { Key = name, Value = field.EnumValue })
                .ToDictionary(a => a.Key, a => a.Value);
        }

        internal static bool TryParse(string value, out TEnum result) => Values.TryGetValue(value, out result);
    }
}