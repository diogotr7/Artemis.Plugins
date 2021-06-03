using Artemis.Core;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Artemis.Plugins.LayerBrushes.Chroma
{
    public class ChromaPluginBootstrapper : PluginBootstrapper
    {
        public override void OnPluginLoaded(Plugin plugin)
        {
            AddPluginPrerequisite(new ChromaSdkPluginPrerequisite(plugin));
        }
    }

    public class ChromaSdkPluginPrerequisite : PluginPrerequisite
    {
        public override string Name => "Chroma SDK";

        public override string Description => "Services needed for Chroma games to send lighting";

        public override List<PluginPrerequisiteAction> InstallActions { get; }

        public override List<PluginPrerequisiteAction> UninstallActions { get; }

        public override bool IsMet()
        {
            const string registryPath = @"SOFTWARE\WOW6432Node\Razer Chroma SDK";
            using RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath);

            return key != null;
        }

        public ChromaSdkPluginPrerequisite(Plugin plugin)
        {
            string downloadUrl = GetDownloadUrlAsync().Result;
            string installerFilename = Path.Combine(plugin.Directory.FullName, "ChromaSdkSetup.exe");

            if (downloadUrl == null)
                throw new Exception();

            InstallActions = new ()
            {
                new DownloadFileAction("Download Chroma SDK installer", downloadUrl, installerFilename),
                new ExecuteFileAction("Install Chroma SDK", installerFilename),
                new DeleteFileAction("Cleanup Chroma SDK installer", installerFilename)
            };

            UninstallActions = new()
            {

            };
        }

        private static async Task<string> GetDownloadUrlAsync()
        {
            using HttpClient httpClient = new();
            const string endpoint = "prod";
            var endpointsJson = await httpClient.GetStringAsync("https://discovery.razerapi.com/user/endpoints");
            var endpoints = JsonConvert.DeserializeObject<RazerRoot>(endpointsJson);
            var prodEndpoint = endpoints.Endpoints.Find(ep => ep.Name == endpoint);
            if (prodEndpoint == null)
                return null;

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

            using var response2 = await httpClient.PostAsync(
                $"https://manifest.razerapi.com/api/legacy/{prodEndpoint.Hash}/{endpoint}/productlist/get",
                new StringContent(platformData, Encoding.UTF8, "application/xml"));
            var a = await response2.Content.ReadAsStringAsync();

            var xml = new XmlDocument();
            xml.LoadXml(a);

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("//Module"))
            {
                if (node["Name"].InnerText == "CHROMABROADCASTER")
                    return node["DownloadURL"].InnerText;
            }

            return null;
        }

        private record RazerEndpoint(string Name, string Hash);
        private record RazerRoot(List<RazerEndpoint> Endpoints);
    }
}
