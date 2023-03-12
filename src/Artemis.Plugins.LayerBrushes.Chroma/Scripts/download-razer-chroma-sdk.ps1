function Get-DownloadUrl {
    $env = "prod"
    $discoveryUrl = "https://discovery.razerapi.com/user/endpoints"
    $discoveryResponse = Invoke-WebRequest -Uri $discoveryUrl -UseBasicParsing
    $discoveryJson = $discoveryResponse.Content | ConvertFrom-Json
    $prodEndpoint = $null

    foreach ($endpoint in $discoveryJson.endpoints) {
        if ($endpoint.name -eq "prod") {
            $prodEndpoint = $endpoint
            break
        }
    }

    #if url is null, then the env is not found
    if ($null -eq $prodEndpoint) {
        Write-Error "Environment not found"
    }
    
    $body = @"
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
"@
    $productListUrl = "https://manifest.razerapi.com/api/legacy/$($prodEndpoint.hash)/$env/productlist/get"
    #body
    $productListResponse = Invoke-WebRequest -Method "POST" -Uri $productListUrl -Body $body -ContentType "application/xml" 

    #load response into xml
    $productListXml = [xml]$productListResponse.Content
    #select nodes '//Module'
    $modules = $productListXml.SelectNodes("//Module")
    #iterate, find "ChromaBroadcast" module
    foreach ($module in $modules) {
        if ($module.Name -eq "CHROMABROADCASTER") {
            return $module.DownloadURL
        }
    }
    return ""
}

$projectDir = $args[0]
$tempDir = "$projectDir\temp"
$targetFileName = "$projectDir\Resources\RazerChromaSdkCoreSetup.exe"

#if file exists, do nothing
if (Test-Path $targetFileName) {
    return
}

#check if 7z is installed
if (!(Get-Command "7z" -ErrorAction SilentlyContinue)) {
    Write-Error "7z is not installed"
    return
}

$url = Get-DownloadUrl
#get file name from url
$downloadedFile = $url.Split("/")[-1]

#download if file does not exist
if (!(Test-Path $downloadedFile)) {
    Invoke-WebRequest -Uri $url -OutFile $downloadedFile
}

#use 7zip to extract
"7z e $downloadedFile -o$tempDir -aoa" | Invoke-Expression

#find in out folder file that starts with "Razer_Chroma_SDK_Core_"
$files = Get-ChildItem -Path $tempDir -Filter "Razer_Chroma_SDK_Core_*"

if ($files.Count -ne 1) {
    Write-Error "Could not find file in out folder"
    return
}

#create Resources folder if it does not exist
if (!(Test-Path "$projectDir/Resources")) {
    New-Item -ItemType Directory -Path "$projectDir\Resources"
}

#copy file to current directory
Copy-Item -Path "$tempDir\$($files[0].Name)" -Destination $targetFileName -Recurse -Force

#remove out folder
Remove-Item -Path $tempDir -Recurse

#remove 7z file
Remove-Item -Path $downloadedFile