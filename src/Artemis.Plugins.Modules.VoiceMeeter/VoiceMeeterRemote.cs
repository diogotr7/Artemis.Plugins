using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.VoiceMeeter
{
    public enum VoiceMeeterLoginResponse
    {
        OK = 0,
        OkVoicemeeterNotRunning = 1,
        NoClient = -1,
        AlreadyLoggedIn = -2,
    }

    public enum VoiceMeeterType
    {
        VoiceMeeter = 1,
        VoiceMeeterBanana = 2,
        VoiceMeeterPotato = 3
    }

    internal static class VoiceMeeterRemote
    {
        private const string DLL = "VoicemeeterRemote64.dll";

        [DllImport(DLL, EntryPoint = "VBVMR_Login", CallingConvention = CallingConvention.StdCall)]
        public static extern VoiceMeeterLoginResponse Login();

        [DllImport(DLL, EntryPoint = "VBVMR_Logout", CallingConvention = CallingConvention.StdCall)]
        public static extern VoiceMeeterLoginResponse Logout();

        [DllImport(DLL, EntryPoint = "VBVMR_GetVoicemeeterType", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetVoicemeeterType(out VoiceMeeterType type);

        [DllImport(DLL, EntryPoint = "VBVMR_GetVoicemeeterVersion", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetVoicemeeterVersion(out int value);


        [DllImport(DLL, EntryPoint = "VBVMR_IsParametersDirty", CallingConvention = CallingConvention.StdCall)]
        public static extern int IsParametersDirty();

        [DllImport(DLL, EntryPoint = "VBVMR_GetParameterFloat", CallingConvention = CallingConvention.StdCall)]
        private static extern int GetParameter([MarshalAs(UnmanagedType.LPStr)] string szParamName, out float value);

        [DllImport(DLL, EntryPoint = "VBVMR_GetParameterStringW", CallingConvention = CallingConvention.StdCall)]
        private static extern int GetParameter([MarshalAs(UnmanagedType.LPStr)] string szParamName, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder value);

        [DllImport(DLL, EntryPoint = "VBVMR_GetLevel", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetLevel(int nType, int nuChannel, out float value);

        public static float GetFloat(string parameter)
        {
            var ret = GetParameter(parameter, out float value);
            //if (ret != 0)
            //    throw new Exception();

            return value;
        }

        public static string GetString(string parameter)
        {
            var sb = new StringBuilder();
            var ret = GetParameter(parameter, sb);
            //if (ret != 0)
            //    throw new Exception();

            return sb.ToString();
        }

        public static bool GetBool(string parameter) => GetFloat(parameter) == 1;

        internal static int GetInt(string parameter) => (int)GetFloat(parameter);
    }
}
