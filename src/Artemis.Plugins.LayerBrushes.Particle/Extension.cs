using SkiaSharp;
using System;

namespace Artemis.Plugins.LayerBrushes.Particle;

public static class Extension
{
    public static float RandomBetween(this Random rnd, float a, float b) => (float)((rnd.NextDouble() * (b - a)) + a);

    public static float GetValueAtPercent(this SKPoint range, float percent) => range.X + ((range.Y - range.X) * percent);
}
