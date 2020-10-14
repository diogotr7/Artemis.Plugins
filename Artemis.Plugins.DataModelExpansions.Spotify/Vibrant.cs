using ColorThiefDotNet;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Artemis.Plugins.DataModelExpansions.Spotify
{
    internal enum ColorType
    {
        Vibrant,
        LightVibrant,
        DarkVibrant,
        Muted,
        LightMuted,
        DarkMuted
    }

    internal static class Vibrant
    {
        private const float targetDarkLuma = 0.26f;
        private const float maxDarkLuma = 0.45f;
        private const float minLightLuma = 0.55f;
        private const float targetLightLuma = 0.74f;
        private const float minNormalLuma = 0.3f;
        private const float targetNormalLuma = 0.5f;
        private const float maxNormalLuma = 0.7f;
        private const float targetMutesSaturation = 0.3f;
        private const float maxMutesSaturation = 0.3f;
        private const float targetVibrantSaturation = 1.0f;
        private const float minVibrantSaturation = 0.35f;
        private const float weightSaturation = 3f;
        private const float weightLuma = 6.5f;
        private const float weightPopulation = 0.5f;

        private static (float targetLuma, float minLuma, float maxLuma, float targetSaturation, float minSaturation, float maxSaturation) GetTargetsForType(ColorType type) => type switch
        {
            ColorType.Vibrant => (targetNormalLuma, minNormalLuma, maxNormalLuma, targetVibrantSaturation, minVibrantSaturation, 1f),
            ColorType.LightVibrant => (targetLightLuma, minLightLuma, 1f, targetVibrantSaturation, minVibrantSaturation, 1f),
            ColorType.DarkVibrant => (targetDarkLuma, 0f, maxDarkLuma, targetVibrantSaturation, minVibrantSaturation, 1f),
            ColorType.Muted => (targetNormalLuma, minNormalLuma, maxNormalLuma, targetMutesSaturation, 0, maxMutesSaturation),
            ColorType.LightMuted => (targetLightLuma, minLightLuma, 1f, targetMutesSaturation, 0, maxMutesSaturation),
            ColorType.DarkMuted => (targetDarkLuma, 0, maxDarkLuma, targetMutesSaturation, 0, maxMutesSaturation),
            _ => throw new NotImplementedException(),
        };

        internal static QuantizedColor FindColorVariation(IEnumerable<QuantizedColor> colors, int maxPopulation, ColorType type)
        {
            var (targetLuma, minLuma, maxLuma, targetSaturation, minSaturation, maxSaturation) = GetTargetsForType(type);

            return colors.OrderByDescending(qc =>
            {
                qc.ToSKColor().ToHsl(out var _, out var s, out var l);
                return GetComparisonValue(s / 100f, targetSaturation, l / 100f, targetLuma, qc.Population, maxPopulation);
            }).FirstOrDefault();
        }

        internal static SKColor FindColorVariation(IEnumerable<SKColor> colors, int maxPopulation, ColorType type)
        {
            var (targetLuma, minLuma, maxLuma, targetSaturation, minSaturation, maxSaturation) = GetTargetsForType(type);

            return colors.OrderByDescending(qc =>
            {
                qc.ToHsl(out var _, out var s, out var l);
                return GetComparisonValue(s / 100f, targetSaturation, l / 100f, targetLuma, 1, maxPopulation);
            }).FirstOrDefault();
        }

        private static float GetComparisonValue(float s, float targetSaturation, float l, float targetLuma, int population, int maxPopulation)
        {
            static float WeightedMean(params float[] values)
            {
                float sum = 0;
                float weightSum = 0;
                for (int i = 0; i < values.Length; i += 2)
                {
                    var value = values[i];
                    var weight = values[i + 1];
                    sum += value * weight;
                    weightSum += weight;
                }

                return sum / weightSum;
            }

            static float InvertDiff(float value, float target)
            {
                return 1 - Math.Abs(value - target);
            }

            return WeightedMean(
                InvertDiff(s, targetSaturation), weightSaturation,
                InvertDiff(l, targetLuma), weightLuma,
                population / maxPopulation, weightPopulation
            );
        }

        internal static SKColor ToSKColor(this QuantizedColor qc)
        {
            if (qc is null) return SKColor.Empty;

            return new SKColor(qc.Color.R, qc.Color.G, qc.Color.B);
        }
    }

    public static class ColorQuantizer
    {
        public static SKColor[] GetQuantizedColors(List<SKColor> pixels, int colorCount)
        {
            if ((colorCount & (colorCount - 1)) != 0)
                throw new ArgumentException("Must be power of two");

            Queue<Cube> cubes = new Queue<Cube>(colorCount);
            cubes.Enqueue(new Cube(pixels));

            while (cubes.Count < colorCount)
            {
                var c = cubes.Dequeue();
                if (c.Split(out var a, out var b))
                {
                    cubes.Enqueue(a);
                    cubes.Enqueue(b);
                }
            }

            return cubes.Select(c => c.GetAverageColor()).ToArray();
        }
    }

    public enum ColorComponent
    {
        Red,
        Green,
        Blue
    }

    public class Cube
    {
        private readonly List<SKColor> _colors;
        private readonly ColorComponent SplitAtComponent;

        public Cube(List<SKColor> colors)
        {
            _colors = colors;

            var redRange = colors.Max(c => c.Red) - colors.Min(c => c.Red);
            var greenRange = colors.Max(c => c.Green) - colors.Min(c => c.Green);
            var blueRange = colors.Max(c => c.Blue) - colors.Min(c => c.Blue);

            if (redRange > greenRange && redRange > blueRange)
                SplitAtComponent = ColorComponent.Red;
            else if (greenRange > redRange && greenRange > blueRange)
                SplitAtComponent = ColorComponent.Green;
            else
                SplitAtComponent = ColorComponent.Blue;
        }

        public bool Split(out Cube cubeA, out Cube cubeB)
        {
            if (_colors.Count < 2)
            {
                cubeA = null;
                cubeB = null;
                return false;
            }

            switch (SplitAtComponent)
            {
                case ColorComponent.Red:
                    _colors.Sort((a, b) => a.Red.CompareTo(b.Red));
                    break;
                case ColorComponent.Green:
                    _colors.Sort((a, b) => a.Green.CompareTo(b.Green));
                    break;
                case ColorComponent.Blue:
                    _colors.Sort((a, b) => a.Blue.CompareTo(b.Blue));
                    break;
            }
            int median = _colors.Count / 2;

            cubeA = new Cube(_colors.GetRange(0, median));
            cubeB = new Cube(_colors.GetRange(median, _colors.Count - median));

            return true;
        }

        public SKColor GetAverageColor()
        {
            int r = 0, g = 0, b = 0;

            for (int i = 0; i < _colors.Count; i++)
            {
                r += _colors[i].Red;
                g += _colors[i].Green;
                b += _colors[i].Blue;
            }

            return new SKColor(
                (byte)(r / _colors.Count),
                (byte)(g / _colors.Count),
                (byte)(b / _colors.Count)
            );
        }
    }
}
