using Artemis.Core.Modules;
using System.Text;

namespace Artemis.Plugins.Modules.VoiceMeeter.DataModels
{
    public class VoiceMeeterStripDataModel : DataModel
    {
        private readonly string _updateHeader;

        public VoiceMeeterStripDataModel(int idx)
        {
            _updateHeader = $"Strip[{idx}]";
        }

        public bool Mono { get; set; }
        public bool Mute { get; set; }
        public bool Solo { get; set; }
        public bool MC { get; set; }
        public float Gain { get; set; }
        //public int[] GainLayer { get; set; }
        public float Pan_x { get; set; }
        public float Pan_y { get; set; }
        public float Color_x { get; set; }
        public float Color_y { get; set; }
        public float fx_x { get; set; }
        public float fx_y { get; set; }
        public int Audibility { get; set; }
        public int Comp { get; set; }
        public int Gate { get; set; }
        public int Karaoke { get; set; }
        public float Limit { get; set; }
        public float EQGain1 { get; set; }
        public float EQGain2 { get; set; }
        public float EQGain3 { get; set; }
        public string Label { get; set; }
        public bool A1 { get; set; }
        public bool A2 { get; set; }
        public bool A3 { get; set; }
        public bool A4 { get; set; }
        public bool A5 { get; set; }
        public bool B1 { get; set; }
        public bool B2 { get; set; }
        public bool B3 { get; set; }
        public string FadeTo { get; set; }
        public string FadeBy { get; set; }
        public int Reverb { get; set; }
        public int Delay { get; set; }
        public int Fx1 { get; set; }
        public int Fx2 { get; set; }
        public bool PostReverb { get; set; }
        public bool PostDelay { get; set; }
        public bool PostFx1 { get; set; }
        public bool PostFx2 { get; set; }

        //public int App[k].Gain { get ; set; }
        //public int App[k].Mute { get; set; }
        //public int AppGain { get; set; }
        //public int AppMute { get; set; }

        public void Update()
        {
            Mono = VoiceMeeterRemote.GetBool($"{_updateHeader}.{nameof(Mono)}");
            Mute = VoiceMeeterRemote.GetBool($"{_updateHeader}.{nameof(Mute)}");
            Solo = VoiceMeeterRemote.GetBool($"{_updateHeader}.{nameof(Solo)}");
            MC = VoiceMeeterRemote.GetBool($"{_updateHeader}.{nameof(MC)}");
            Gain = VoiceMeeterRemote.GetFloat($"{_updateHeader}.{nameof(Gain)}");
            Pan_x = VoiceMeeterRemote.GetFloat($"{_updateHeader}.{nameof(Pan_x)}");
            Pan_y = VoiceMeeterRemote.GetFloat($"{_updateHeader}.{nameof(Pan_y)}");
            Color_x = VoiceMeeterRemote.GetFloat($"{_updateHeader}.{nameof(Color_x)}");
            Color_y = VoiceMeeterRemote.GetFloat($"{_updateHeader}.{nameof(Color_y)}");
            fx_x = VoiceMeeterRemote.GetFloat($"{_updateHeader}.{nameof(fx_x)}");
            fx_y = VoiceMeeterRemote.GetFloat($"{_updateHeader}.{nameof(fx_y)}");
            Audibility = VoiceMeeterRemote.GetInt($"{_updateHeader}.{nameof(Audibility)}");
            Comp = VoiceMeeterRemote.GetInt($"{_updateHeader}.{nameof(Comp)}");
            Gate = VoiceMeeterRemote.GetInt($"{_updateHeader}.{nameof(Gate)}");
            Karaoke = VoiceMeeterRemote.GetInt($"{_updateHeader}.{nameof(Karaoke)}");
            Limit = VoiceMeeterRemote.GetFloat($"{_updateHeader}.{nameof(Limit)}");
            EQGain1 = VoiceMeeterRemote.GetFloat($"{_updateHeader}.{nameof(EQGain1)}");
            EQGain2 = VoiceMeeterRemote.GetFloat($"{_updateHeader}.{nameof(EQGain2)}");
            EQGain3 = VoiceMeeterRemote.GetFloat($"{_updateHeader}.{nameof(EQGain3)}");
            Label = VoiceMeeterRemote.GetString($"{_updateHeader}.{nameof(Label)}");
            A1 = VoiceMeeterRemote.GetBool($"{_updateHeader}.{nameof(A1)}");
            A2 = VoiceMeeterRemote.GetBool($"{_updateHeader}.{nameof(A2)}");
            A3 = VoiceMeeterRemote.GetBool($"{_updateHeader}.{nameof(A3)}");
            A4 = VoiceMeeterRemote.GetBool($"{_updateHeader}.{nameof(A4)}");
            A5 = VoiceMeeterRemote.GetBool($"{_updateHeader}.{nameof(A5)}");
            B1 = VoiceMeeterRemote.GetBool($"{_updateHeader}.{nameof(B1)}");
            B2 = VoiceMeeterRemote.GetBool($"{_updateHeader}.{nameof(B2)}");
            B3 = VoiceMeeterRemote.GetBool($"{_updateHeader}.{nameof(B3)}");
            FadeTo = VoiceMeeterRemote.GetString($"{_updateHeader}.{nameof(FadeTo)}");
            FadeBy = VoiceMeeterRemote.GetString($"{_updateHeader}.{nameof(FadeBy)}");
            Reverb = VoiceMeeterRemote.GetInt($"{_updateHeader}.{nameof(Reverb)}");
            Delay = VoiceMeeterRemote.GetInt($"{_updateHeader}.{nameof(Delay)}");
            Fx1 = VoiceMeeterRemote.GetInt($"{_updateHeader}.{nameof(Fx1)}");
            Fx2 = VoiceMeeterRemote.GetInt($"{_updateHeader}.{nameof(Fx2)}");
            PostReverb = VoiceMeeterRemote.GetBool($"{_updateHeader}.{nameof(PostReverb)}");
            PostDelay = VoiceMeeterRemote.GetBool($"{_updateHeader}.{nameof(PostDelay)}");
            PostFx1 = VoiceMeeterRemote.GetBool($"{_updateHeader}.{nameof(PostFx1)}");
            PostFx2 = VoiceMeeterRemote.GetBool($"{_updateHeader}.{nameof(PostFx2)}");
        }
    }
}