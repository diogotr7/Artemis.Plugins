﻿using Artemis.Core.LayerBrushes;
using Artemis.Plugins.LayerBrushes.Gif.ViewModels;
using Artemis.UI.Shared.Services;

namespace Artemis.Plugins.LayerBrushes.Gif
{
    // This is your plugin, it provides Artemis wil one or more layer effects via descriptors.
    // Your plugin gets enabled once. Your layer effects get enabled multiple times, once for each profile element (folder/layer) it is applied to.
    public class GifLayerBrushProvider : LayerBrushProvider
    {
        private readonly IProfileEditorService _profileEditorService;

        public GifLayerBrushProvider(IProfileEditorService profileEditorService)
        {
            _profileEditorService = profileEditorService;
        }

        public override void EnablePlugin()
        {
            // This is where we can register our effect for use, we can also register multiple effects if we'd like
            //_profileEditorService.RegisterPropertyInput<FilePathPropertyInputViewModel>(PluginInfo);
            RegisterLayerBrushDescriptor<GifLayerBrush>("Gif layer brush", "Gif layer brush", "Gif");
        }

        public override void DisablePlugin()
        {
            // Any registrations we made will be removed automatically, we don't need to do anything here
        }
    }
}