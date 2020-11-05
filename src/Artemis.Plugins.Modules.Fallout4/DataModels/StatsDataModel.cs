namespace Artemis.Plugins.Modules.Fallout4.DataModels
{
    public class StatsDataModel
    {
        internal DictAccessor<float> lLegCondition;
        internal DictAccessor<uint> radawayCount;
        internal DictAccessor<float> headCondition;
        internal DictAccessor<uint> stimpakCount;
        internal DictAccessor<float> rLegCondition;
        internal DictAccessor<float> torsoCondition;
        internal DictAccessor<float> rArmCondition;
        internal DictAccessor<uint> bodyFlags;
        internal DictAccessor<uint> headFlags;
        internal DictAccessor<float> lArmCondition;

        public float LLegCondition => lLegCondition?.Value ?? default;
        public uint RadawayCount => radawayCount?.Value ?? default;
        public float HeadCondition => headCondition?.Value ?? default;
        public uint StimpakCount => stimpakCount?.Value ?? default;
        public float RLegCondition => rLegCondition?.Value ?? default;
        public float TorsoCondition => torsoCondition?.Value ?? default;
        public float RArmCondition => rArmCondition?.Value ?? default;
        public uint BodyFlags => bodyFlags?.Value ?? default;
        public uint HeadFlags => headFlags?.Value ?? default;
        public float LArmCondition => lArmCondition?.Value ?? default;
    }
}