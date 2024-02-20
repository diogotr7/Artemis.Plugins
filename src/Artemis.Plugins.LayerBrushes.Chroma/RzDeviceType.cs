using RazerSdkReader.Structures;

namespace Artemis.Plugins.LayerBrushes.Chroma;

public enum RzDeviceType
{
    Mousepad,
    Mouse,
    Keypad,
    Keyboard,
    Headset,
    ChromaLink,
}

public static class EnumExtensions
{
    public static string ToStringFast(this RzDeviceType value) => value switch
    {
        RzDeviceType.Mousepad => nameof(RzDeviceType.Mousepad),
        RzDeviceType.Mouse => nameof(RzDeviceType.Mouse),
        RzDeviceType.Keypad => nameof(RzDeviceType.Keypad),
        RzDeviceType.Keyboard => nameof(RzDeviceType.Keyboard),
        RzDeviceType.Headset => nameof(RzDeviceType.Headset),
        RzDeviceType.ChromaLink => nameof(RzDeviceType.ChromaLink),
        _ => value.ToString(),
    };

    public static int GetLength(this RzDeviceType value) => value switch
    {
        RzDeviceType.Mousepad => ChromaMousepad.COUNT,
        RzDeviceType.Mouse => ChromaMouse.COUNT,
        RzDeviceType.Keypad => ChromaKeypad.COUNT,
        RzDeviceType.Keyboard => ChromaKeyboard.COUNT,
        RzDeviceType.Headset => ChromaHeadset.COUNT,
        RzDeviceType.ChromaLink => ChromaLink.COUNT,
        _ => 0,
    };
}