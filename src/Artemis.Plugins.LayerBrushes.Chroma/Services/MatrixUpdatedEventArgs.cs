using System;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.Chroma.Services;

public sealed class MatrixUpdatedEventArgs : EventArgs
{
    public required RzDeviceType DeviceType { get; init; }
    public required SKColor[,] Matrix { get; init; }
}