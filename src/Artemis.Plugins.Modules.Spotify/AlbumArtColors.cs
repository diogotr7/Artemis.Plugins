using Artemis.Core;
using Artemis.Core.ColorScience;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.Spotify;

public class AlbumArtColors : IDisposable
{
    private readonly PluginSetting<Dictionary<string, ColorSwatch>> _cache;
    private readonly HttpClient _httpClient = new() {Timeout = TimeSpan.FromSeconds(2)};

    public AlbumArtColors(PluginSettings settings)
    {
        _cache = settings.GetSetting<Dictionary<string, ColorSwatch>>("AlbumArtCache", new());
    }

    public async Task<ColorSwatch> Get(string albumArtUrl)
    {
        string key = albumArtUrl.Split('/').Last();
        lock (_cache)
        {
            if (_cache.Value!.TryGetValue(key, out ColorSwatch swatch))
                return swatch;
        }

        ColorSwatch newSwatch = await GetFromUrl(albumArtUrl);
        lock (_cache)
        {
            _cache.Value![key] = newSwatch;
            _cache.Save();
        }

        return newSwatch;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    private async Task<ColorSwatch> GetFromUrl(string url)
    {
        using Stream stream = await _httpClient.GetStreamAsync(url);
        using SKBitmap skbm = SKBitmap.Decode(stream);
        SKColor[] skClrs = ColorQuantizer.Quantize(skbm.Pixels, 256);
        return ColorQuantizer.FindAllColorVariations(skClrs, true);
    }
}
