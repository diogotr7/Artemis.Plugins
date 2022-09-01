﻿using Artemis.Core.Modules;
using System;

namespace Artemis.Plugins.Modules.Discord.DataModels
{
    public class DiscordUserDataModel : DataModel
    {
        public string Username { get; set; }

        public string Discriminator { get; set; }

        public string Id { get; set; }

        internal void Apply(User user)
        {
            Username = user.Username;
            Discriminator = user.Discriminator;
            Id = user.Id;
        }
    }
}