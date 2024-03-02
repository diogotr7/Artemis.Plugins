using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Artemis.Core;

namespace Artemis.Plugins.LayerBrushes.Chroma.Prerequisites;

public class DownloadAndInstallChromaSdkAction : PluginPrerequisiteAction
{
    public DownloadAndInstallChromaSdkAction() : base("Install Chroma SDK")
    {
    }

    public override async Task Execute(CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();
        var downloadLink = await GetDownloadUrlAsync(httpClient);
        var tempFolder = Path.GetTempPath();
        var tempPath = Path.Combine(tempFolder, "RazerChromaSdkInstaller.exe");
        
        using (var response = await httpClient.GetStreamAsync(downloadLink, cancellationToken))
        {
            await using (var fileStream = new FileStream(tempPath, FileMode.Create))
            {
                await response.CopyToAsync(fileStream, cancellationToken);
            }
        }

        var childAction = new ExecuteFileAction("Install Chroma SDK", tempPath, elevate: true, arguments: "/S", waitForExit: true);
        await childAction.Execute(cancellationToken);

        File.Delete(tempPath);
    }

    private async Task<string> GetDownloadUrlAsync(HttpClient httpClient)
    {
        //a bunch of nullable overrides here but i don't care
        const string ENDPOINT = "prod";
        var endpointsJson = await httpClient.GetStringAsync("https://discovery.razerapi.com/user/endpoints");
        var endpoints = JsonSerializer.Deserialize<RazerRoot>(endpointsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var prodEndpoint = endpoints!.endpoints.Find(ep => ep.name == ENDPOINT);

        const string PLATFORM_DATA = """
                                     <PlatformRoot xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
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
                                     """;
        using var response2 = await httpClient.PostAsync(
            $"https://manifest.razerapi.com/api/legacy/{prodEndpoint!.hash}/{ENDPOINT}/productlist/get",
            new StringContent(PLATFORM_DATA, Encoding.UTF8, "application/xml"));
        var a = await response2.Content.ReadAsStringAsync();

        var xml = new XmlDocument();
        xml.LoadXml(a);

        foreach (XmlNode node in xml!.DocumentElement!.SelectNodes("//Module")!)
        {
            if (node["Name"]!.InnerText == "CHROMABROADCASTER")
                return node["DownloadURL"]!.InnerText;
        }

        throw new ArtemisPluginException("Failed to retrieve Razer API download URL");
    }

    private record RazerEndpoint(string name, string hash);

    private record RazerRoot(List<RazerEndpoint> endpoints);
}