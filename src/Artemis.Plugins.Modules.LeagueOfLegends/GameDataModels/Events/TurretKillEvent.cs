﻿namespace Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels
{
    public class TurretKillEvent : LolEvent
    {
        public string KillerName { get; set; }
        public string TurretKilled { get; set; }
        public string[] Assisters { get; set; }
    }
}
