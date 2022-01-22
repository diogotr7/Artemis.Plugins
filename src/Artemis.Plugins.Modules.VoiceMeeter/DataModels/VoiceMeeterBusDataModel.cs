using Artemis.Core.Modules;
using System;

namespace Artemis.Plugins.Modules.VoiceMeeter.DataModels
{
    public class VoiceMeeterBusDataModel : DataModel
    {
        private readonly string _updateHeader;

        public VoiceMeeterBusDataModel(int idx)
        {
            _updateHeader = $"Bus[{idx}]";
        }

        public int Mono { get; set; }
        public bool Mute { get; set; }
        public bool EQon { get; set; }
        public bool EQAB { get; set; }
        public float Gain { get; set; }
        public int normal { get; set; }
        public int Amix { get; set; }
        public int Bmix { get; set; }
        public int Repeat { get; set; }
        public int Composite { get; set; }
        public int TVMix { get; set; }
        public int UpMix21 { get; set; }
        public int UpMix41 { get; set; }
        public int UpMix61 { get; set; }
        public int CenterOnly { get; set; }
        public int LFEOnly { get; set; }
        public int RearOnly { get; set; }
        //public int EQ.channel[j].cell[k].on { get; set; }
        //public int EQ.channel[j].cell[k].type { get; set; }
        //public int EQ.channel[j].cell[k].f  { get; set; }
        //public int EQ.channel[j].cell[k].gain  { get; set; }
        //public int EQ.channel[j].cell[k].q  { get; set; }
        public string FadeTo { get; set; }
        public string FadeBy { get; set; }
        public bool Sel { get; set; }
        public int ReturnReverb { get; set; }
        public int ReturnDelay { get; set; }
        public int ReturnFx1 { get; set; }
        public int ReturnFx2 { get; set; }
        public int Monitor { get; set; }

        internal void Update()
        {
            Mono = VoiceMeeterRemote.GetInt($"{_updateHeader}.{nameof(Mono)}");
            Mute = VoiceMeeterRemote.GetBool($"{_updateHeader}.{nameof(Mute)}");
            EQon = VoiceMeeterRemote.GetBool($"{_updateHeader}.{"EQ.On"}");
            EQAB = VoiceMeeterRemote.GetBool($"{_updateHeader}.{"EQ.AB"}");
            Gain = VoiceMeeterRemote.GetFloat($"{_updateHeader}.{nameof(Gain)}");
            
            //bus modes
            normal = VoiceMeeterRemote.GetInt($"{_updateHeader}.mode.{nameof(normal)}");
            Amix = VoiceMeeterRemote.GetInt($"{_updateHeader}.mode.{nameof(Amix)}");
            Bmix = VoiceMeeterRemote.GetInt($"{_updateHeader}.mode.{nameof(Bmix)}");
            Repeat = VoiceMeeterRemote.GetInt($"{_updateHeader}.mode.{nameof(Repeat)}");
            Composite = VoiceMeeterRemote.GetInt($"{_updateHeader}.mode.{nameof(Composite)}");
            TVMix = VoiceMeeterRemote.GetInt($"{_updateHeader}.mode.{nameof(TVMix)}");
            UpMix21 = VoiceMeeterRemote.GetInt($"{_updateHeader}.mode.{nameof(UpMix21)}");
            UpMix41 = VoiceMeeterRemote.GetInt($"{_updateHeader}.mode.{nameof(UpMix41)}");
            UpMix61 = VoiceMeeterRemote.GetInt($"{_updateHeader}.mode.{nameof(UpMix61)}");
            CenterOnly = VoiceMeeterRemote.GetInt($"{_updateHeader}.mode.{nameof(CenterOnly)}");
            LFEOnly = VoiceMeeterRemote.GetInt($"{_updateHeader}.mode.{nameof(LFEOnly)}");
            RearOnly = VoiceMeeterRemote.GetInt($"{_updateHeader}.mode.{nameof(RearOnly)}");

            FadeTo = VoiceMeeterRemote.GetString($"{_updateHeader}.{nameof(FadeTo)}");
            FadeBy = VoiceMeeterRemote.GetString($"{_updateHeader}.{nameof(FadeBy)}");
            Sel = VoiceMeeterRemote.GetBool($"{_updateHeader}.{nameof(Sel)}");
            ReturnReverb = VoiceMeeterRemote.GetInt($"{_updateHeader}.{nameof(ReturnReverb)}");
            ReturnDelay = VoiceMeeterRemote.GetInt($"{_updateHeader}.{nameof(ReturnDelay)}");
            ReturnFx1 = VoiceMeeterRemote.GetInt($"{_updateHeader}.{nameof(ReturnFx1)}");
            ReturnFx2 = VoiceMeeterRemote.GetInt($"{_updateHeader}.{nameof(ReturnFx2)}");
            Monitor = VoiceMeeterRemote.GetInt($"{_updateHeader}.{nameof(Monitor)}");
        }
    }
}