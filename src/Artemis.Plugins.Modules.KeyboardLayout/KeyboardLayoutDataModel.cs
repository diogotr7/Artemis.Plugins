using Artemis.Core.Modules;

namespace Artemis.Plugins.Modules.KeyboardLayout;

public class KeyboardLayoutDataModel : DataModel
{
    public int Hkl { get; set; }
    public string Klid { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}