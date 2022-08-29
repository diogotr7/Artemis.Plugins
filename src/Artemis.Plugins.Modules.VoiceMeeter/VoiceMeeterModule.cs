using System;
using System.Collections.Generic;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Modules.VoiceMeeter.DataModels;

namespace Artemis.Plugins.Modules.VoiceMeeter
{
    [PluginFeature(AlwaysEnabled = true, Name = "VoiceMeeter")]

    public class VoiceMeeterModule : Module<VoiceMeeterDataModel>
    {
        private VoiceMeeterStripDataModel[] _strips;
        private VoiceMeeterBusDataModel[] _busses;

        public override List<IModuleActivationRequirement> ActivationRequirements { get; }
        = new List<IModuleActivationRequirement> { new ProcessActivationRequirement("voicemeeter8x64") };

        public override void Enable()
        {
            var vmLogin = VoiceMeeterRemote.Login();
            if (vmLogin != VoiceMeeterLoginResponse.OK)
                throw new ArtemisPluginException("Error connecting to voicemeeter");

            GetInitialInformation();
        }

        public override void Update(double deltaTime)
        {
            if (VoiceMeeterRemote.IsParametersDirty() == 1)
            {
                UpdateParameters();
            }
        }

        public override void Disable()
        {
            VoiceMeeterRemote.Logout();
        }

        private void GetInitialInformation()
        {
            VoiceMeeterRemote.GetVoicemeeterVersion(out var version);

            var v1 = (version & 0xFF000000) >> 24;
            var v2 = (version & 0x00FF0000) >> 16;
            var v3 = (version & 0x0000FF00) >> 8;
            var v4 = version & 0x000000FF;

            DataModel.Information.VoiceMeeterVersion = $"{v1}.{v2}.{v3}.{v4}";

            VoiceMeeterRemote.GetVoicemeeterType(out var voiceMeeterType);

            DataModel.Information.VoiceMeeterType = voiceMeeterType;
            DataModel.Information.StripCount = GetStripCount(voiceMeeterType);
            DataModel.Information.BusCount = GetBusCount(voiceMeeterType);

            _strips = new VoiceMeeterStripDataModel[DataModel.Information.StripCount];
            for (int i = 0; i < DataModel.Information.StripCount; i++)
            {
                var strip = new VoiceMeeterStripDataModel(i);
                strip.Update();
                _strips[i] = DataModel.Strips.AddDynamicChild($"Strip {i + 1}", strip).Value;
            }

            _busses = new VoiceMeeterBusDataModel[DataModel.Information.BusCount];
            for (int i = 0; i < DataModel.Information.BusCount; i++)
            {
                var bus = new VoiceMeeterBusDataModel(i);
                bus.Update();
                _busses[i] = DataModel.Busses.AddDynamicChild($"Bus {i + 1}", bus).Value;
            }
        }

        private void UpdateParameters()
        {
            foreach (var strip in _strips)
            {
                strip.Update();
            }

            foreach (var bus in _busses)
            {
                bus.Update();
            }

            //TODO: Levels
        }

        private static int GetStripCount(VoiceMeeterType type) => type switch
        {
            VoiceMeeterType.VoiceMeeter => 3,
            VoiceMeeterType.VoiceMeeterBanana => 5,
            VoiceMeeterType.VoiceMeeterPotato => 8,
            _ => 0,
        };

        private static int GetBusCount(VoiceMeeterType type) => type switch
        {
            VoiceMeeterType.VoiceMeeter => 2,
            VoiceMeeterType.VoiceMeeterBanana => 5,
            VoiceMeeterType.VoiceMeeterPotato => 8,
            _ => 0,
        };
    }
}
