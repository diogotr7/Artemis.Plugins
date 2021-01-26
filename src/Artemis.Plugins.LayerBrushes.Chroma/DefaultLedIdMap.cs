using RazerSdkWrapper.Data;
using RGB.NET.Core;
using System;
using System.Collections.Generic;

namespace Artemis.Plugins.LayerBrushes.Chroma
{
    //https://developer.razer.com/works-with-chroma/razer-chroma-led-profiles/
    internal static class DefaultLedIdMap
    {
        private static readonly Dictionary<(int Row, int Column), LedId> Keyboard = new Dictionary<(int Row, int Column), LedId>
        {
            [(0, 1)] = LedId.Keyboard_Escape,
            [(0, 3)] = LedId.Keyboard_F1,
            [(0, 4)] = LedId.Keyboard_F2,
            [(0, 5)] = LedId.Keyboard_F3,
            [(0, 6)] = LedId.Keyboard_F4,
            [(0, 7)] = LedId.Keyboard_F5,
            [(0, 8)] = LedId.Keyboard_F6,
            [(0, 9)] = LedId.Keyboard_F7,
            [(0, 10)] = LedId.Keyboard_F8,
            [(0, 11)] = LedId.Keyboard_F9,
            [(0, 12)] = LedId.Keyboard_F10,
            [(0, 13)] = LedId.Keyboard_F11,
            [(0, 14)] = LedId.Keyboard_F12,
            [(0, 15)] = LedId.Keyboard_PrintScreen,
            [(0, 16)] = LedId.Keyboard_ScrollLock,
            [(0, 17)] = LedId.Keyboard_PauseBreak,
            [(0, 20)] = LedId.Logo,

            [(1, 0)] = LedId.Keyboard_Macro1,
            [(1, 1)] = LedId.Keyboard_GraveAccentAndTilde,
            [(1, 2)] = LedId.Keyboard_1,
            [(1, 3)] = LedId.Keyboard_2,
            [(1, 4)] = LedId.Keyboard_3,
            [(1, 5)] = LedId.Keyboard_4,
            [(1, 6)] = LedId.Keyboard_5,
            [(1, 7)] = LedId.Keyboard_6,
            [(1, 8)] = LedId.Keyboard_7,
            [(1, 9)] = LedId.Keyboard_8,
            [(1, 10)] = LedId.Keyboard_9,
            [(1, 11)] = LedId.Keyboard_0,
            [(1, 12)] = LedId.Keyboard_MinusAndUnderscore,
            [(1, 13)] = LedId.Keyboard_EqualsAndPlus,
            [(1, 14)] = LedId.Keyboard_Backspace,
            [(1, 15)] = LedId.Keyboard_Insert,
            [(1, 16)] = LedId.Keyboard_Home,
            [(1, 17)] = LedId.Keyboard_PageUp,
            [(1, 18)] = LedId.Keyboard_NumLock,
            [(1, 19)] = LedId.Keyboard_NumSlash,
            [(1, 20)] = LedId.Keyboard_NumAsterisk,
            [(1, 21)] = LedId.Keyboard_NumMinus,

            [(2, 0)] = LedId.Keyboard_Macro2,
            [(2, 1)] = LedId.Keyboard_Tab,
            [(2, 2)] = LedId.Keyboard_Q,
            [(2, 3)] = LedId.Keyboard_W,
            [(2, 4)] = LedId.Keyboard_E,
            [(2, 5)] = LedId.Keyboard_R,
            [(2, 6)] = LedId.Keyboard_T,
            [(2, 7)] = LedId.Keyboard_Y,
            [(2, 8)] = LedId.Keyboard_U,
            [(2, 9)] = LedId.Keyboard_I,
            [(2, 10)] = LedId.Keyboard_O,
            [(2, 11)] = LedId.Keyboard_P,
            [(2, 12)] = LedId.Keyboard_BracketLeft,
            [(2, 13)] = LedId.Keyboard_BracketRight,
            [(2, 14)] = LedId.Keyboard_Backslash,
            [(2, 15)] = LedId.Keyboard_Delete,
            [(2, 16)] = LedId.Keyboard_End,
            [(2, 17)] = LedId.Keyboard_PageDown,
            [(2, 18)] = LedId.Keyboard_Num7,
            [(2, 19)] = LedId.Keyboard_Num8,
            [(2, 20)] = LedId.Keyboard_Num9,
            [(2, 21)] = LedId.Keyboard_NumPlus,

            [(3, 0)] = LedId.Keyboard_Macro3,
            [(3, 1)] = LedId.Keyboard_CapsLock,
            [(3, 2)] = LedId.Keyboard_A,
            [(3, 3)] = LedId.Keyboard_S,
            [(3, 4)] = LedId.Keyboard_D,
            [(3, 5)] = LedId.Keyboard_F,
            [(3, 6)] = LedId.Keyboard_G,
            [(3, 7)] = LedId.Keyboard_H,
            [(3, 8)] = LedId.Keyboard_J,
            [(3, 9)] = LedId.Keyboard_K,
            [(3, 10)] = LedId.Keyboard_L,
            [(3, 11)] = LedId.Keyboard_SemicolonAndColon,
            [(3, 12)] = LedId.Keyboard_ApostropheAndDoubleQuote,
            [(3, 13)] = LedId.Keyboard_NonUsTilde,
            [(3, 14)] = LedId.Keyboard_Enter,
            [(3, 18)] = LedId.Keyboard_Num4,
            [(3, 19)] = LedId.Keyboard_Num5,
            [(3, 20)] = LedId.Keyboard_Num6,

            [(4, 0)] = LedId.Keyboard_Macro4,
            [(4, 1)] = LedId.Keyboard_LeftShift,
            [(4, 2)] = LedId.Keyboard_NonUsBackslash,
            [(4, 3)] = LedId.Keyboard_Z,
            [(4, 4)] = LedId.Keyboard_X,
            [(4, 5)] = LedId.Keyboard_C,
            [(4, 6)] = LedId.Keyboard_V,
            [(4, 7)] = LedId.Keyboard_B,
            [(4, 8)] = LedId.Keyboard_N,
            [(4, 9)] = LedId.Keyboard_M,
            [(4, 10)] = LedId.Keyboard_CommaAndLessThan,
            [(4, 11)] = LedId.Keyboard_PeriodAndBiggerThan,
            [(4, 12)] = LedId.Keyboard_SlashAndQuestionMark,
            [(4, 14)] = LedId.Keyboard_RightShift,
            [(4, 16)] = LedId.Keyboard_ArrowUp,
            [(4, 18)] = LedId.Keyboard_Num1,
            [(4, 19)] = LedId.Keyboard_Num2,
            [(4, 20)] = LedId.Keyboard_Num3,
            [(4, 21)] = LedId.Keyboard_NumEnter,

            [(5, 0)] = LedId.Keyboard_Macro5,
            [(5, 1)] = LedId.Keyboard_LeftCtrl,
            [(5, 2)] = LedId.Keyboard_LeftGui,
            [(5, 3)] = LedId.Keyboard_LeftAlt,
            [(5, 7)] = LedId.Keyboard_Space,
            [(5, 11)] = LedId.Keyboard_RightAlt,
            [(5, 12)] = LedId.Keyboard_RightGui,
            [(5, 13)] = LedId.Keyboard_Application,
            [(5, 14)] = LedId.Keyboard_RightCtrl,
            [(5, 15)] = LedId.Keyboard_ArrowLeft,
            [(5, 16)] = LedId.Keyboard_ArrowDown,
            [(5, 17)] = LedId.Keyboard_ArrowRight,
            [(5, 19)] = LedId.Keyboard_Num0,
            [(5, 20)] = LedId.Keyboard_NumPeriodAndDelete
        };

        private static readonly Dictionary<(int Row, int Column), LedId> Mouse = new Dictionary<(int Row, int Column), LedId>
        {
            [(1, 0)] = LedId.Mouse1,
            [(1, 6)] = LedId.Mouse2,

            [(2, 0)] = LedId.Mouse3,
            [(2, 3)] = LedId.Mouse4,
            [(2, 6)] = LedId.Mouse5,

            [(3, 0)] = LedId.Mouse6,
            [(3, 6)] = LedId.Mouse7,

            [(4, 0)] = LedId.Mouse8,
            [(4, 3)] = LedId.Mouse9,
            [(4, 6)] = LedId.Mouse10,

            [(5, 0)] = LedId.Mouse11,
            [(5, 6)] = LedId.Mouse12,

            [(6, 0)] = LedId.Mouse13,
            [(6, 6)] = LedId.Mouse14,

            [(7, 0)] = LedId.Mouse15,
            [(7, 3)] = LedId.Mouse16,
            [(7, 6)] = LedId.Mouse17,

            [(8, 1)] = LedId.Mouse18,
            [(8, 2)] = LedId.Mouse19,
            [(8, 3)] = LedId.Mouse20,
            [(8, 4)] = LedId.Mouse21,
            [(8, 5)] = LedId.Mouse22,
            [(8, 6)] = LedId.Mouse23,
        };

        private static readonly Dictionary<(int Row, int Column), LedId> Mousepad = new Dictionary<(int Row, int Column), LedId>
        {
            [(0, 0)] = LedId.Mousepad15,
            [(0, 1)] = LedId.Mousepad14,
            [(0, 2)] = LedId.Mousepad13,
            [(0, 3)] = LedId.Mousepad12,
            [(0, 4)] = LedId.Mousepad11,
            [(0, 5)] = LedId.Mousepad10,
            [(0, 6)] = LedId.Mousepad9,
            [(0, 7)] = LedId.Mousepad8,
            [(0, 8)] = LedId.Mousepad7,
            [(0, 9)] = LedId.Mousepad6,
            [(0, 10)] = LedId.Mousepad5,
            [(0, 11)] = LedId.Mousepad4,
            [(0, 12)] = LedId.Mousepad3,
            [(0, 13)] = LedId.Mousepad2,
            [(0, 14)] = LedId.Mousepad1
        };

        private static readonly Dictionary<(int Row, int Column), LedId> Headset = new Dictionary<(int Row, int Column), LedId>
        {
            [(0, 0)] = LedId.Headset1,
            [(0, 1)] = LedId.Headset2,
            [(0, 2)] = LedId.Headset3,
            [(0, 3)] = LedId.Headset4,
            [(0, 4)] = LedId.Headset5
        };

        private static readonly Dictionary<(int Row, int Column), LedId> Keypad = new Dictionary<(int Row, int Column), LedId>
        {
            [(0, 0)] = LedId.Keypad1,
            [(0, 1)] = LedId.Keypad2,
            [(0, 2)] = LedId.Keypad3,
            [(0, 3)] = LedId.Keypad4,
            [(0, 4)] = LedId.Keypad5,

            [(1, 0)] = LedId.Keypad6,
            [(1, 1)] = LedId.Keypad7,
            [(1, 2)] = LedId.Keypad8,
            [(1, 3)] = LedId.Keypad9,
            [(1, 4)] = LedId.Keypad10,

            [(2, 0)] = LedId.Keypad11,
            [(2, 1)] = LedId.Keypad12,
            [(2, 2)] = LedId.Keypad13,
            [(2, 3)] = LedId.Keypad14,
            [(2, 4)] = LedId.Keypad15,

            [(3, 0)] = LedId.Keypad16,
            [(3, 1)] = LedId.Keypad17,
            [(3, 2)] = LedId.Keypad18,
            [(3, 3)] = LedId.Keypad19,
            [(3, 4)] = LedId.Keypad20,
        };

        private static readonly Dictionary<(int Row, int Column), LedId> ChromaLink = new Dictionary<(int Row, int Column), LedId>
        {
            [(0, 0)] = LedId.LedStripe1,
            [(0, 1)] = LedId.LedStripe2,
            [(0, 2)] = LedId.LedStripe3,
            [(0, 3)] = LedId.LedStripe4,
            [(0, 4)] = LedId.LedStripe5,
        };

        internal static Dictionary<RzDeviceType, Dictionary<(int Row, int Column), LedId>> DeviceTypes = new Dictionary<RzDeviceType, Dictionary<(int Row, int Column), LedId>>
        {
            [RzDeviceType.Keyboard] = Keyboard,
            [RzDeviceType.Mouse] = Mouse,
            [RzDeviceType.Mousepad] = Mousepad,
            [RzDeviceType.Headset] = Headset,
            [RzDeviceType.Keypad] = Keypad,
            [RzDeviceType.ChromaLink] = ChromaLink
        };
    }
}
