using System;

namespace Artemis.Plugins.LayerBrushes.Particle
{
    public static class Extension
    {
        public static float RandomBetween(this Random rnd, float a, float b) => (float)((rnd.NextDouble() * (b - a)) + a);
    }
}
