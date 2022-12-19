using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Modules.VoiceMeeter.DataModels;

namespace Artemis.Plugins.Modules.VoiceMeeter;

[PluginFeature(AlwaysEnabled = true, Name = "VoiceMeeter")]
public class VoiceMeeterModule : Module<VoiceMeeterDataModel>
{
    public override List<IModuleActivationRequirement> ActivationRequirements { get; } = new()
    {
        new ProcessActivationRequirement("voicemeeter"),    //voicemeeter
        new ProcessActivationRequirement("voicemeeterpro"), //voicemeeter banana
        new ProcessActivationRequirement("voicemeeter8"),   //voicemeeter potato 32 bit
        new ProcessActivationRequirement("voicemeeter8x64"),//voicemeeter potato 64 bit
    };

    public override void Enable()
    {
        var vmLogin = VoiceMeeterRemote.Login();
        if (vmLogin != VoiceMeeterLoginResponse.OK && vmLogin != VoiceMeeterLoginResponse.AlreadyLoggedIn)
            throw new ArtemisPluginException($"Error connecting to voicemeeter: {vmLogin}");

        GetInitialInformation();
    }

    public override void Update(double deltaTime)
    {
        if (VoiceMeeterRemote.IsParametersDirty() == 1)
            UpdateParameters();

        foreach (var level in DataModel.Levels.InputLevels.DynamicChildren.Values.OfType<DynamicChild<VoiceMeeterLevelDataModel>>())
            level.Value.Update();

        foreach (var level in DataModel.Levels.OutputLevels.DynamicChildren.Values.OfType< DynamicChild<VoiceMeeterLevelDataModel>>())
            level.Value.Update();
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
        DataModel.Information.PhysicalStripCount = GetPhysicalStripCount(voiceMeeterType);

        for (int i = 0; i < DataModel.Information.StripCount; i++)
            DataModel.Strips.AddDynamicChild($"Strip {i + 1}", new VoiceMeeterStripDataModel(i));

        for (int i = 0; i < DataModel.Information.BusCount; i++)
            DataModel.Busses.AddDynamicChild($"Bus {i + 1}", new VoiceMeeterBusDataModel(i));

        for (int i = 0; i < DataModel.Information.StripCount; i++)
        {
            const int POST_MUTE_INPUT_LEVELS = 2;
            if (i <= DataModel.Information.PhysicalStripCount)
            {
                var count = CHANNELS_PER_STRIP * i;
                DataModel.Levels.InputLevels.AddDynamicChild($"Input Strip Level {i + 1}", new VoiceMeeterLevelDataModel(POST_MUTE_INPUT_LEVELS, count, CHANNELS_PER_STRIP));
            }
            else
            {
                var count = CHANNELS_PER_STRIP * DataModel.Information.PhysicalStripCount + CHANNELS_PER_BUS * (i - DataModel.Information.PhysicalStripCount);
                DataModel.Levels.InputLevels.AddDynamicChild($"Input Bus Level {i + 1}", new VoiceMeeterLevelDataModel(POST_MUTE_INPUT_LEVELS, count, CHANNELS_PER_BUS));
            }
        }

        const int OUTPUT_LEVELS = 3;
        for (int i = 0; i < DataModel.Information.BusCount; i++)
            DataModel.Levels.OutputLevels.AddDynamicChild($"Output Bus Level {i + 1}", new VoiceMeeterLevelDataModel(OUTPUT_LEVELS, i * CHANNELS_PER_BUS, CHANNELS_PER_BUS));

    }
    
    private void UpdateParameters()
    {
        foreach (var strip in DataModel.Strips.DynamicChildren.Values.OfType<VoiceMeeterStripDataModel>())
            strip.Update();

        foreach (var bus in DataModel.Busses.DynamicChildren.Values.OfType<VoiceMeeterBusDataModel>())
            bus.Update();
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

    private const int CHANNELS_PER_STRIP = 2;
    private const int CHANNELS_PER_BUS = 8;

    private static int GetPhysicalStripCount(VoiceMeeterType type) => type switch
    {
        VoiceMeeterType.VoiceMeeter => 2,
        VoiceMeeterType.VoiceMeeterBanana => 3,
        VoiceMeeterType.VoiceMeeterPotato => 5,
        _ => 0,
    };
}