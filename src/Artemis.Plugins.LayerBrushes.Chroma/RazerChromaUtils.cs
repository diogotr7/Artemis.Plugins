using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Artemis.Plugins.LayerBrushes.Chroma
{
    public static class RazerChromaUtils
    {
        private record RazerEndpoint(string Name, string Hash);
        private record RazerRoot(List<RazerEndpoint> Endpoints);
        private static readonly HttpClient httpClient = new();

        public static async Task<string> GetDownloadUrlAsync()
        {
            const string endpoint = "prod";
            var endpointsJson = await httpClient.GetStringAsync("https://discovery.razerapi.com/user/endpoints");
            var endpoints = JsonConvert.DeserializeObject<RazerRoot>(endpointsJson);
            var prodEndpoint = endpoints.Endpoints.Find(ep => ep.Name == endpoint);
            if (prodEndpoint == null)
                throw new Exception("Failed to find prod endpoint");

            const string platformData = @"
<PlatformRoot xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <Platform>
    <Arch>64</Arch>
    <Locale>en</Locale>
    <Mfr>Generic-MFR</Mfr>
    <Model>Generic-MDL</Model>
    <OS>Windows</OS>
    <OSVer>10</OSVer>
    <SKU>Generic-SKU</SKU>
  </Platform>
</PlatformRoot>
";

            using var productListXml = await httpClient.PostAsync(
                $"https://manifest.razerapi.com/api/legacy/{prodEndpoint.Hash}/{endpoint}/productlist/get",
                new StringContent(platformData, Encoding.UTF8, "application/xml"));

            var productList = new XmlDocument();
            productList.LoadXml(await productListXml.Content.ReadAsStringAsync());

            foreach (XmlNode node in productList.DocumentElement.SelectNodes("//Module"))
            {
                if (node["Name"].InnerText == "CHROMABROADCASTER")
                    return node["DownloadURL"].InnerText;
            }

            throw new Exception("Failed to find Chroma Broadcaster URL");
        }

        public static async Task<string> DownloadAsync()
        {
            var url = await GetDownloadUrlAsync();
            if (url == null)
                return null;

            var path = Path.ChangeExtension(Path.GetTempFileName(), ".exe");

            using var fileStream = File.OpenWrite(path);
            using var res = await httpClient.GetAsync(url);
            await res.Content.CopyToAsync(fileStream);

            return path;
        }

        public static async Task<int> InstallAsync(string installerPath)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = Path.GetFileName(installerPath),
                WorkingDirectory = Path.GetDirectoryName(installerPath),
                Arguments = "/S",
                ErrorDialog = true
            };
            var cancel = new CancellationTokenSource();
            cancel.CancelAfter(120000);

            var process = Process.Start(processInfo);
            await process.WaitForExitAsync(cancel.Token);

            return process.ExitCode;
        }

        public static string[] GetRazerPriorityList()
        {
            var localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            var razerChroma = localMachine.OpenSubKey(@"Software\Razer Chroma SDK");
            var razerChromaPriorityList = razerChroma.GetValue("PriorityList");
            if (razerChromaPriorityList != null && razerChromaPriorityList is string s)
            {
                return s.Split(';');
            }

            var apps = razerChroma.OpenSubKey("Apps");
            if (apps != null)
            {
                var appsPriorityList = apps.GetValue("PriorityList");
                if (appsPriorityList is string st)
                {
                    return st.Split(';');
                }
            }

            return Array.Empty<string>();
        }
    }
}
