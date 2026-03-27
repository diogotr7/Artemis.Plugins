function Get-DownloadUrl {
    $installerManifestUrl = "https://apps.razer.com/chroma-app/dashboard/installer-manifest.json"
    $installerManifestResponse = Invoke-WebRequest -Uri $installerManifestUrl -UseBasicParsing
    $installerManifest = $installerManifestResponse.Content | ConvertFrom-Json

    if ($null -eq $installerManifest -or [string]::IsNullOrWhiteSpace($installerManifest.baseURL) -or $null -eq $installerManifest.latest -or [string]::IsNullOrWhiteSpace($installerManifest.latest.url)) {
        Write-Error "Failed to retrieve installer manifest"
        return ""
    }

    $manifestPath = "{0}/{1}" -f $installerManifest.baseURL.TrimStart('/'), $installerManifest.latest.url.TrimStart('/')
    $latestManifestUrl = [System.Uri]::new([System.Uri]::new("https://apps.razer.com/"), $manifestPath).AbsoluteUri

    $latestManifestResponse = Invoke-WebRequest -Uri $latestManifestUrl -UseBasicParsing
    $latestManifest = $latestManifestResponse.Content | ConvertFrom-Json

    if ($null -eq $latestManifest -or $null -eq $latestManifest.resources) {
        Write-Error "Failed to retrieve latest Chroma manifest"
        return ""
    }

    foreach ($resource in $latestManifest.resources) {
        if ($resource.resourceName -eq "ExtraInstaller_Razer Chroma SDK Core") {
            return $resource.url
        }
    }

    Write-Error "Chroma SDK Core installer resource not found"
    return ""
}

$projectDir = $args[0]
$targetFileName = "$projectDir\Resources\RazerChromaSdkCoreSetup.exe"

#if file exists, do nothing
if (Test-Path $targetFileName) {
    return
}

$url = Get-DownloadUrl
if ([string]::IsNullOrWhiteSpace($url)) {
    return
}

#create Resources folder if it does not exist
if (!(Test-Path "$projectDir/Resources")) {
    New-Item -ItemType Directory -Path "$projectDir\Resources"
}

Invoke-WebRequest -Uri $url -OutFile $targetFileName