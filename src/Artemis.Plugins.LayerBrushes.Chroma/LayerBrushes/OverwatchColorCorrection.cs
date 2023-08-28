﻿using System.Collections.Generic;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.Chroma.LayerBrushes;

public static class OverwatchColorCorrection
{
    /// <summary>
    /// Maps Overwatch background colors to the more vibrant peripheral equivalent.
    /// These colors may also be used to detect which hero is currently selected.
    /// </summary>
    public static Dictionary<SKColor, SKColor> ColorMap = new()
    {
        [SKColor.Parse("#371e00")] = SKColor.Parse("#dd7900"), //Base,
        [SKColor.Parse("#3f1e2f")] = SKColor.Parse("#fc79bd"), //Dva,
        [SKColor.Parse("#190703")] = SKColor.Parse("#661e0f"), //Doomfist,
        [SKColor.Parse("#152733")] = SKColor.Parse("#579fcf"), //JunkerQueen,
        [SKColor.Parse("#041b01")] = SKColor.Parse("#106f04"), //Orisa,
        [SKColor.Parse("#1f1531")] = SKColor.Parse("#7d55c7"), //Rammatra,
        [SKColor.Parse("#1f2223")] = SKColor.Parse("#7c8b8c"), //Reinhardt,
        [SKColor.Parse("#2b1b07")] = SKColor.Parse("#ae6f1c"), //Roadhog,
        [SKColor.Parse("#1f2223")] = SKColor.Parse("#7c8b8c"), //Sigma,
        [SKColor.Parse("#23242b")] = SKColor.Parse("#8f92ae"), //Winston,
        [SKColor.Parse("#381e02")] = SKColor.Parse("#e2790a"), //WreckingBall,
        [SKColor.Parse("#3d1729")] = SKColor.Parse("#f65ea6"), //Zarya,
        [SKColor.Parse("#0f0f0e")] = SKColor.Parse("#3e3c3a"), //Sshe,
        [SKColor.Parse("#161c14")] = SKColor.Parse("#5b7351"), //Bastion,
        [SKColor.Parse("#290a09")] = SKColor.Parse("#a62927"), //Cassidy,
        [SKColor.Parse("#22323f")] = SKColor.Parse("#89c8ff"), //Echo,
        [SKColor.Parse("#203e00")] = SKColor.Parse("#80fb00"), //Genji,
        [SKColor.Parse("#2c2a19")] = SKColor.Parse("#b2a865"), //Hanzo,
        [SKColor.Parse("#3d2c05")] = SKColor.Parse("#f7b217"), //Junkrat,
        [SKColor.Parse("#11263c")] = SKColor.Parse("#469af0"), //Mei,
        [SKColor.Parse("#00162f")] = SKColor.Parse("#0058bc"), //Pharah,
        [SKColor.Parse("#170006")] = SKColor.Parse("#5e001a"), //Reaper,
        [SKColor.Parse("#350f0b")] = SKColor.Parse("#d73e2c"), //Sojourn,
        [SKColor.Parse("#11141d")] = SKColor.Parse("#445275"), //Soldier76,
        [SKColor.Parse("#140a2a")] = SKColor.Parse("#5128a9"), //Sombra,
        [SKColor.Parse("#1d2d32")] = SKColor.Parse("#76b4c9"), //Symmetra,
        [SKColor.Parse("#2e130f")] = SKColor.Parse("#ba4c3f"), //Torbjorn,
        [SKColor.Parse("#371e00")] = SKColor.Parse("#de7a00"), //Tracer,
        [SKColor.Parse("#220f23")] = SKColor.Parse("#8b3f8f"), //Widowmaker,
        [SKColor.Parse("#121a27")] = SKColor.Parse("#48699e"), //Ana,
        [SKColor.Parse("#0a2930")] = SKColor.Parse("#28a5c3"), //Baptiste,
        [SKColor.Parse("#1c0c0a")] = SKColor.Parse("#72332a"), //Brigitte,
        [SKColor.Parse("#292315")] = SKColor.Parse("#a58c54"), //Illari,
        [SKColor.Parse("#341115")] = SKColor.Parse("#d04656"), //Kiriko,
        [SKColor.Parse("#38292e")] = SKColor.Parse("#e1a5ba"), //Lifeweaver,
        [SKColor.Parse("#193106")] = SKColor.Parse("#67c519"), //Lucio,
        [SKColor.Parse("#3e3c2b")] = SKColor.Parse("#faf2ad"), //Mercy,
        [SKColor.Parse("#201239")] = SKColor.Parse("#804be5"), //Moira,
        [SKColor.Parse("#3f3b16")] = SKColor.Parse("#fcee5a"), //Zenyatta,      
    };

    public static Dictionary<SKColor, OverwatchHeroes> HeroesMap = new()
    {
        [SKColor.Parse("#dd7900")] = OverwatchHeroes.Base,
        [SKColor.Parse("#fc79bd")] = OverwatchHeroes.Dva,
        [SKColor.Parse("#661e0f")] = OverwatchHeroes.Doomfist,
        [SKColor.Parse("#579fcf")] = OverwatchHeroes.JunkerQueen,
        [SKColor.Parse("#106f04")] = OverwatchHeroes.Orisa,
        [SKColor.Parse("#7d55c7")] = OverwatchHeroes.Rammatra,
        [SKColor.Parse("#7c8b8c")] = OverwatchHeroes.Reinhardt,
        [SKColor.Parse("#ae6f1c")] = OverwatchHeroes.Roadhog,
        //[SKColor.Parse("#7c8b8c")] = OverwatchHeroes.Sigma,
        [SKColor.Parse("#8f92ae")] = OverwatchHeroes.Winston,
        [SKColor.Parse("#e2790a")] = OverwatchHeroes.WreckingBall,
        [SKColor.Parse("#f65ea6")] = OverwatchHeroes.Zarya,
        [SKColor.Parse("#3e3c3a")] = OverwatchHeroes.Ashe,
        [SKColor.Parse("#5b7351")] = OverwatchHeroes.Bastion,
        [SKColor.Parse("#a62927")] = OverwatchHeroes.Cassidy,
        [SKColor.Parse("#89c8ff")] = OverwatchHeroes.Echo,
        [SKColor.Parse("#80fb00")] = OverwatchHeroes.Genji,
        [SKColor.Parse("#b2a865")] = OverwatchHeroes.Hanzo,
        [SKColor.Parse("#f7b217")] = OverwatchHeroes.Junkrat,
        [SKColor.Parse("#469af0")] = OverwatchHeroes.Mei,
        [SKColor.Parse("#0058bc")] = OverwatchHeroes.Pharah,
        [SKColor.Parse("#5e001a")] = OverwatchHeroes.Reaper,
        [SKColor.Parse("#d73e2c")] = OverwatchHeroes.Sojourn,
        [SKColor.Parse("#445275")] = OverwatchHeroes.Soldier76,
        [SKColor.Parse("#5128a9")] = OverwatchHeroes.Sombra,
        [SKColor.Parse("#76b4c9")] = OverwatchHeroes.Symmetra,
        [SKColor.Parse("#ba4c3f")] = OverwatchHeroes.Torbjorn,
        [SKColor.Parse("#de7a00")] = OverwatchHeroes.Tracer,
        [SKColor.Parse("#8b3f8f")] = OverwatchHeroes.Widowmaker,
        [SKColor.Parse("#48699e")] = OverwatchHeroes.Ana,
        [SKColor.Parse("#28a5c3")] = OverwatchHeroes.Baptiste,
        [SKColor.Parse("#72332a")] = OverwatchHeroes.Brigitte,
        [SKColor.Parse("#a58c54")] = OverwatchHeroes.Illari,
        [SKColor.Parse("#d04656")] = OverwatchHeroes.Kiriko,
        [SKColor.Parse("#e1a5ba")] = OverwatchHeroes.Lifeweaver,
        [SKColor.Parse("#67c519")] = OverwatchHeroes.Lucio,
        [SKColor.Parse("#faf2ad")] = OverwatchHeroes.Mercy,
        [SKColor.Parse("#804be5")] = OverwatchHeroes.Moira,
        [SKColor.Parse("#fcee5a")] = OverwatchHeroes.Zenyatta,      
    };

    public enum OverwatchHeroes
    {
        Base,
        Dva,
        Doomfist,
        JunkerQueen,
        Orisa,
        Rammatra,
        Reinhardt,
        Roadhog,
        Sigma,
        Winston,
        WreckingBall,
        Zarya,
        Ashe,
        Bastion,
        Cassidy,
        Echo,
        Genji,
        Hanzo,
        Junkrat,
        Mei,
        Pharah,
        Reaper,
        Sojourn,
        Soldier76,
        Sombra,
        Symmetra,
        Torbjorn,
        Tracer,
        Widowmaker,
        Ana,
        Baptiste,
        Brigitte,
        Illari,
        Kiriko,
        Lifeweaver,
        Lucio,
        Mercy,
        Moira,
        Zenyatta,    
    }
}