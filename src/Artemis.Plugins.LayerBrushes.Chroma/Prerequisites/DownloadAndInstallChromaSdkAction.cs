using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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
		var downloadLink = await GetDownloadUrlAsync(httpClient, cancellationToken);
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

	private async Task<string> GetDownloadUrlAsync(HttpClient httpClient, CancellationToken cancellationToken)
	{
		var jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

		var installerManifestJson = await httpClient.GetStringAsync("https://apps.razer.com/chroma-app/dashboard/installer-manifest.json", cancellationToken);
		var installerManifest = JsonSerializer.Deserialize<InstallerManifest>(installerManifestJson, jsonSerializerOptions);

		if (string.IsNullOrWhiteSpace(installerManifest?.baseURL) || string.IsNullOrWhiteSpace(installerManifest.latest?.url))
			throw new ArtemisPluginException("Failed to retrieve Razer installer manifest");

		var latestManifestPath = $"{installerManifest.baseURL.TrimStart('/')}/{installerManifest.latest.url.TrimStart('/')}";
		var latestManifestUrl = new Uri(new Uri("https://apps.razer.com/"), latestManifestPath);

		var latestManifestJson = await httpClient.GetStringAsync(latestManifestUrl, cancellationToken);
		var latestManifest = JsonSerializer.Deserialize<ChromaManifest>(latestManifestJson, jsonSerializerOptions);

		var installerResource = latestManifest?.resources?.Find(resource => resource.resourceName == "ExtraInstaller_Razer Chroma SDK Core");
		if (string.IsNullOrWhiteSpace(installerResource?.url))
			throw new ArtemisPluginException("Failed to retrieve Razer Chroma SDK Core download URL");

		return installerResource.url;
	}

	private record InstallerManifest(string baseURL, InstallerManifestRelease latest);

	private record InstallerManifestRelease(string url);

	private record ChromaManifest(List<ChromaResource> resources);

	private record ChromaResource(string resourceName, string url);
}