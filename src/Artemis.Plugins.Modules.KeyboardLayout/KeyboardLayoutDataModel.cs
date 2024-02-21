using Artemis.Core.Modules;

namespace Artemis.Plugins.Modules.KeyboardLayout;

public class KeyboardLayoutDataModel : DataModel
{
    public uint Hkl { get; set; }
    public ushort HklLowWord { get; set; }
    public ushort HklHighWord { get; set; }
    public string Klid { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}