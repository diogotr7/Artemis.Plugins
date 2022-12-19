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

        UpdateLevels();
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

        var totalStripChannels = 0;
        for (int i = 0; i < DataModel.Information.StripCount; i++)
        {
            int channels = i < GetPhysicalStripCount(DataModel.Information.VoiceMeeterType) ? 
                        CHANNELS_PER_PHYSICAL_STRIP : CHANNELS_PER_VIRTUAL_STRIP;

            var level = new VoiceMeeterLevelDataModel(POST_MUTE_INPUT_LEVELS, totalStripChannels, channels);
            var strip = new VoiceMeeterStripDataModel(i, level);
            
            strip.Update();
            strip.UpdateLevels();

            var name = string.IsNullOrWhiteSpace(strip.Label) ? $"Strip {i + 1}" : strip.Label;
            DataModel.Strips.AddDynamicChild(i.ToString(), strip, name);

            totalStripChannels += channels;
        }

        var names = GetBusNames(DataModel.Information.VoiceMeeterType).ToArray();
        for (int i = 0; i < DataModel.Information.BusCount; i++)
        {
            var level = new VoiceMeeterLevelDataModel(OUTPUT_LEVELS, CHANNELS_PER_BUS * i, CHANNELS_PER_BUS);
            var bus = new VoiceMeeterBusDataModel(i, level);
            
            bus.Update();
            bus.UpdateLevels();

            DataModel.Busses.AddDynamicChild(i.ToString(), bus, names[i]);
        }
    }
    
    private void UpdateParameters()
    {
        foreach (var strip in DataModel.Strips.DynamicChildren.Values.OfType<DynamicChild<VoiceMeeterStripDataModel>>())
            strip.Value.Update();

        foreach (var bus in DataModel.Busses.DynamicChildren.Values.OfType<DynamicChild<VoiceMeeterBusDataModel>>())
            bus.Value.Update();
    }

    private void UpdateLevels()
    {
        foreach (var strip in DataModel.Strips.DynamicChildren.Values.OfType<DynamicChild<VoiceMeeterStripDataModel>>())
            strip.Value.UpdateLevels();

        foreach (var bus in DataModel.Busses.DynamicChildren.Values.OfType<DynamicChild<VoiceMeeterBusDataModel>>())
            bus.Value.UpdateLevels();
    }

    private static int GetPhysicalStripCount(VoiceMeeterType type) => type switch
    {
        VoiceMeeterType.VoiceMeeter => 2,
        VoiceMeeterType.VoiceMeeterBanana => 3,
        VoiceMeeterType.VoiceMeeterPotato => 5,
        _ => 0,
    };

    private static int GetVirtualStripCount(VoiceMeeterType type) => type switch
    {
        VoiceMeeterType.VoiceMeeter => 1,
        VoiceMeeterType.VoiceMeeterBanana => 2,
        VoiceMeeterType.VoiceMeeterPotato => 3,
        _ => 0,
    };

    private static int GetPhysicalBusCount(VoiceMeeterType type) => type switch
    {
        VoiceMeeterType.VoiceMeeter => 1,
        VoiceMeeterType.VoiceMeeterBanana => 3,
        VoiceMeeterType.VoiceMeeterPotato => 5,
        _ => 0,
    };

    private static int GetVirtualBusCount(VoiceMeeterType type) => type switch
    {
        VoiceMeeterType.VoiceMeeter => 1,
        VoiceMeeterType.VoiceMeeterBanana => 2,
        VoiceMeeterType.VoiceMeeterPotato => 3,
        _ => 0,
    };
    
    private static int GetStripCount(VoiceMeeterType type) => GetPhysicalStripCount(type) + GetVirtualStripCount(type);
    
    private static int GetBusCount(VoiceMeeterType type) => GetPhysicalBusCount(type) + GetVirtualBusCount(type);

    private const int CHANNELS_PER_PHYSICAL_STRIP = 2;
    private const int CHANNELS_PER_VIRTUAL_STRIP = 8;
    private const int CHANNELS_PER_BUS = 8;

    private const int PRE_FADER_INPUT_LEVELS = 0;
	private const int POST_FADER_INPUT_LEVELS = 1;
	private const int POST_MUTE_INPUT_LEVELS = 2;
	private const int OUTPUT_LEVELS = 3;

    private static IEnumerable<string> GetBusNames(VoiceMeeterType type)
    {
        foreach (var item in Enumerable.Range(0, GetPhysicalBusCount(type)))
            yield return $"A{item + 1}";
        
        foreach (var item in Enumerable.Range(0, GetVirtualBusCount(type)))
            yield return $"B{item + 1}";
    }
}