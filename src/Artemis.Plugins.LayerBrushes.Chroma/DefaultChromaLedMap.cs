using Artemis.Plugins.LayerBrushes.Chroma.ChromaService;
using RGB.NET.Core;
using System.Collections.Generic;

namespace Artemis.Plugins.LayerBrushes.Chroma;

//https://developer.razer.com/works-with-chroma/razer-chroma-led-profiles/
public static class DefaultChromaLedMap
{
    public static readonly LedId[,] Keyboard =
    {
        {
            LedId.Invalid,
            LedId.Keyboard_Escape,
            LedId.Invalid,
            LedId.Keyboard_F1,
            LedId.Keyboard_F2,
            LedId.Keyboard_F3,
            LedId.Keyboard_F4,
            LedId.Keyboard_F5,
            LedId.Keyboard_F6,
            LedId.Keyboard_F7,
            LedId.Keyboard_F8,
            LedId.Keyboard_F9,
            LedId.Keyboard_F10,
            LedId.Keyboard_F11,
            LedId.Keyboard_F12,
            LedId.Keyboard_PrintScreen,
            LedId.Keyboard_ScrollLock,
            LedId.Keyboard_PauseBreak,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Logo,
            LedId.Invalid
        },
        {
            LedId.Keyboard_Macro1,
            LedId.Keyboard_GraveAccentAndTilde,
            LedId.Keyboard_1,
            LedId.Keyboard_2,
            LedId.Keyboard_3,
            LedId.Keyboard_4,
            LedId.Keyboard_5,
            LedId.Keyboard_6,
            LedId.Keyboard_7,
            LedId.Keyboard_8,
            LedId.Keyboard_9,
            LedId.Keyboard_0,
            LedId.Keyboard_MinusAndUnderscore,
            LedId.Keyboard_EqualsAndPlus,
            LedId.Keyboard_Backspace,
            LedId.Keyboard_Insert,
            LedId.Keyboard_Home,
            LedId.Keyboard_PageUp,
            LedId.Keyboard_NumLock,
            LedId.Keyboard_NumSlash,
            LedId.Keyboard_NumAsterisk,
            LedId.Keyboard_NumMinus
        },
        {
            LedId.Keyboard_Macro2,
            LedId.Keyboard_Tab,
            LedId.Keyboard_Q,
            LedId.Keyboard_W,
            LedId.Keyboard_E,
            LedId.Keyboard_R,
            LedId.Keyboard_T,
            LedId.Keyboard_Y,
            LedId.Keyboard_U,
            LedId.Keyboard_I,
            LedId.Keyboard_O,
            LedId.Keyboard_P,
            LedId.Keyboard_BracketLeft,
            LedId.Keyboard_BracketRight,
            LedId.Keyboard_Backslash,
            LedId.Keyboard_Delete,
            LedId.Keyboard_End,
            LedId.Keyboard_PageDown,
            LedId.Keyboard_Num7,
            LedId.Keyboard_Num8,
            LedId.Keyboard_Num9,
            LedId.Keyboard_NumPlus
        },
        {
            LedId.Keyboard_Macro3,
            LedId.Keyboard_CapsLock,
            LedId.Keyboard_A,
            LedId.Keyboard_S,
            LedId.Keyboard_D,
            LedId.Keyboard_F,
            LedId.Keyboard_G,
            LedId.Keyboard_H,
            LedId.Keyboard_J,
            LedId.Keyboard_K,
            LedId.Keyboard_L,
            LedId.Keyboard_SemicolonAndColon,
            LedId.Keyboard_ApostropheAndDoubleQuote,
            LedId.Keyboard_NonUsTilde,
            LedId.Keyboard_Enter,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Keyboard_Num4,
            LedId.Keyboard_Num5,
            LedId.Keyboard_Num6,
            LedId.Invalid
        },
        {
            LedId.Keyboard_Macro4,
            LedId.Keyboard_LeftShift,
            LedId.Keyboard_NonUsBackslash,
            LedId.Keyboard_Z,
            LedId.Keyboard_X,
            LedId.Keyboard_C,
            LedId.Keyboard_V,
            LedId.Keyboard_B,
            LedId.Keyboard_N,
            LedId.Keyboard_M,
            LedId.Keyboard_CommaAndLessThan,
            LedId.Keyboard_PeriodAndBiggerThan,
            LedId.Keyboard_SlashAndQuestionMark,
            LedId.Invalid,
            LedId.Keyboard_RightShift,
            LedId.Invalid,
            LedId.Keyboard_ArrowUp,
            LedId.Invalid,
            LedId.Keyboard_Num1,
            LedId.Keyboard_Num2,
            LedId.Keyboard_Num3,
            LedId.Keyboard_NumEnter
        },
        {
            LedId.Keyboard_Macro5,
            LedId.Keyboard_LeftCtrl,
            LedId.Keyboard_LeftGui,
            LedId.Keyboard_LeftAlt,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Keyboard_Space,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Keyboard_RightAlt,
            LedId.Keyboard_Function,
            LedId.Keyboard_Application,
            LedId.Keyboard_RightCtrl,
            LedId.Keyboard_ArrowLeft,
            LedId.Keyboard_ArrowDown,
            LedId.Keyboard_ArrowRight,
            LedId.Invalid,
            LedId.Keyboard_Num0,
            LedId.Keyboard_NumPeriodAndDelete,
            LedId.Invalid
        }
    };

    public static readonly LedId[,] Mouse =
    {
        {
            LedId.Invalid,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Invalid
        },
        {
            LedId.Mouse1,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Mouse2
        },
        {
            LedId.Mouse3,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Mouse4,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Mouse5
        },
        {
            LedId.Mouse6,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Mouse7
        },
        {
            LedId.Mouse8,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Mouse9,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Mouse10
        },
        {
            LedId.Mouse11,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Mouse12
        },
        {
            LedId.Mouse13,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Mouse14
        },
        {
            LedId.Mouse15,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Mouse16,
            LedId.Invalid,
            LedId.Invalid,
            LedId.Mouse17
        },
        {
            LedId.Invalid,
            LedId.Mouse18,
            LedId.Mouse19,
            LedId.Mouse20,
            LedId.Mouse21,
            LedId.Mouse22,
            LedId.Invalid
        }
    };

    public static readonly LedId[,] Mousepad =
    {
        {
            LedId.Mousepad15,
            LedId.Mousepad14,
            LedId.Mousepad13,
            LedId.Mousepad12,
            LedId.Mousepad11,
            LedId.Mousepad10,
            LedId.Mousepad9,
            LedId.Mousepad8,
            LedId.Mousepad7,
            LedId.Mousepad6,
            LedId.Mousepad5,
            LedId.Mousepad4,
            LedId.Mousepad3,
            LedId.Mousepad2,
            LedId.Mousepad1
        }
    };

    public static readonly LedId[,] Headset =
    {
        {
            LedId.Headset1,
            LedId.Headset2,
            LedId.Headset3,
            LedId.Headset4,
            LedId.Headset5
        }
    };

    public static readonly LedId[,] Keypad =
    {
        {
            LedId.Keypad1,
            LedId.Keypad2,
            LedId.Keypad3,
            LedId.Keypad4,
            LedId.Keypad5
        },
        {
            LedId.Keypad6,
            LedId.Keypad7,
            LedId.Keypad8,
            LedId.Keypad9,
            LedId.Keypad10
        },
        {
            LedId.Keypad11,
            LedId.Keypad12,
            LedId.Keypad13,
            LedId.Keypad14,
            LedId.Keypad15
        },
        {
            LedId.Keypad16,
            LedId.Keypad17,
            LedId.Keypad18,
            LedId.Keypad19,
            LedId.Keypad20
        }
    };

    public static readonly LedId[,] ChromaLink =
    {
        {
            LedId.LedStripe1,
            LedId.LedStripe2,
            LedId.LedStripe3,
            LedId.LedStripe4,
            LedId.LedStripe5
        }
    };

    public static readonly Dictionary<RzDeviceType, LedId[,]> DeviceTypes = new Dictionary<RzDeviceType, LedId[,]>
    {
        [RzDeviceType.Keyboard] = Keyboard,
        [RzDeviceType.Mouse] = Mouse,
        [RzDeviceType.Mousepad] = Mousepad,
        [RzDeviceType.Headset] = Headset,
        [RzDeviceType.Keypad] = Keypad,
        [RzDeviceType.ChromaLink] = ChromaLink
    };

    public static Dictionary<RzDeviceType, LedId[,]> Clone() => new Dictionary<RzDeviceType, LedId[,]>
    {
        [RzDeviceType.Keyboard] = Keyboard.Clone() as LedId[,],
        [RzDeviceType.Mouse] = Mouse.Clone() as LedId[,],
        [RzDeviceType.Mousepad] = Mousepad.Clone() as LedId[,],
        [RzDeviceType.Headset] = Headset.Clone() as LedId[,],
        [RzDeviceType.Keypad] = Keypad.Clone() as LedId[,],
        [RzDeviceType.ChromaLink] = ChromaLink.Clone() as LedId[,],
    };
}
