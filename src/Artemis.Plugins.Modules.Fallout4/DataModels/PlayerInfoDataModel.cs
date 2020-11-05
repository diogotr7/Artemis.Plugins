namespace Artemis.Plugins.Modules.Fallout4.DataModels
{
    public class PlayerInfoDataModel
    {
        internal DictAccessor<float> maxWeight;
        internal DictAccessor<int> xPLevel;
        internal DictAccessor<uint> dateYear;
        internal DictAccessor<float> maxHP;
        internal DictAccessor<float> currWeight;
        internal DictAccessor<uint> perkPoints;
        internal DictAccessor<float> maxAP;
        internal DictAccessor<float> currHP;
        internal DictAccessor<string> playerName;
        internal DictAccessor<float> xPProgressPct;
        internal DictAccessor<sbyte> dateDay;
        internal DictAccessor<float> timeHour;
        internal DictAccessor<int> caps;
        internal DictAccessor<float> currentHPGain;
        internal DictAccessor<uint> dateMonth;
        internal DictAccessor<float> currAP;

        public float MaxWeight => maxWeight?.Value ?? default;
        public int XPLevel => xPLevel?.Value ?? default;
        public uint DateYear => dateYear?.Value ?? default;
        public float MaxHP => maxHP?.Value ?? default;
        public float CurrWeight => currWeight?.Value ?? default;
        public uint PerkPoints => perkPoints?.Value ?? default;
        public float MaxAP => maxAP?.Value ?? default;
        public float CurrHP => currHP?.Value ?? default;
        public string PlayerName => playerName?.Value ?? default;
        public float XPProgressPct => xPProgressPct?.Value ?? default;
        public sbyte DateDay => dateDay?.Value ?? default;
        public float TimeHour => timeHour?.Value ?? default;
        public int Caps => caps?.Value ?? default;
        public float CurrentHPGain => currentHPGain?.Value ?? default;
        public uint DateMonth => dateMonth?.Value ?? default;
        public float CurrAP => currAP?.Value ?? default;
    }
}