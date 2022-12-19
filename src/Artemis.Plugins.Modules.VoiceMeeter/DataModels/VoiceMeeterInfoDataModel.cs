namespace Artemis.Plugins.Modules.VoiceMeeter.DataModels
{
    public class VoiceMeeterInfoDataModel
    {
        public VoiceMeeterType VoiceMeeterType { get; set; }
        public string VoiceMeeterVersion { get; set; }
        public int StripCount { get; set; }
        public int BusCount { get; set; }
        public int PhysicalStripCount { get; internal set; }
    }
}