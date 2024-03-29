﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Artemis.Core.Modules;

namespace Artemis.Plugins.Modules.KeyboardLayout;

public class KeyboardLayoutModule : Module<KeyboardLayoutDataModel>
{
    public override List<IModuleActivationRequirement>? ActivationRequirements { get; } = new();

    public override void Enable()
    {
        AddTimedUpdate(TimeSpan.FromMilliseconds(300), UpdateKeyboardLayout); 
    }

    public override void Disable()
    {
    }

    public override void Update(double deltaTime)
    {
    }
    
    private const int KL_NAMELENGTH = 32;

    [DllImport("user32.dll", EntryPoint = "GetKeyboardLayoutNameA", CharSet = CharSet.Ansi)]
    private static extern bool GetKeyboardLayoutNameA([Out] StringBuilder pwszKLID);

    [DllImport("user32.dll")]
    private static extern nint GetKeyboardLayout(uint idThread);

    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(nint hWnd, nint ProcessId);

    [DllImport("user32.dll")]
    private static extern int ActivateKeyboardLayout(uint HKL, int flags);

    public void UpdateKeyboardLayout(double deltaTime)
    {
        var hkl = GetHkl();
        var lowWord = (ushort)hkl;
        var highWord = (ushort)(hkl >> 16);
        var klid = GetKlidFromHkl(hkl);
        var name = GetNameFromKlid(klid);
        
        DataModel.Hkl = hkl;
        DataModel.Klid = klid;
        DataModel.Name = name;
        DataModel.HklLowWord = lowWord;
        DataModel.HklHighWord = highWord;
    }

    private static uint GetHkl()
    {
        return (uint)GetKeyboardLayout(GetWindowThreadProcessId(GetForegroundWindow(), nint.Zero));
    }

    private readonly Dictionary<string, string> _klidToName = new();
    public string GetNameFromKlid(string klid)
    {
        if (_klidToName.TryGetValue(klid, out var x))
            return x;
        
        using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Keyboard Layouts\" + klid);
        var r = key?.GetValue("Layout Text") as string ?? klid;
        _klidToName.Add(klid, r);
        return r;
    }
    
    private readonly Dictionary<string, string> _hklToKlid = new();
    public string GetKlidFromHkl(uint hkl)
    {
        if (_hklToKlid.TryGetValue(hkl.ToString(), out var x))
            return x;
        
        var previousLayout = ActivateKeyboardLayout(hkl, 100);
        if (previousLayout == 0)
            return string.Empty;

        StringBuilder sb = new(KL_NAMELENGTH);
        GetKeyboardLayoutNameA(sb);
        _hklToKlid.Add(hkl.ToString(), sb.ToString());
        return sb.ToString();
    }
}