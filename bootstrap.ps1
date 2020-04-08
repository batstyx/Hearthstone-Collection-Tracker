if (Test-Path "nuget.exe") {
	Invoke-Expression "./nuget restore"
}
elseif((Get-Command "nuget" -ErrorAction SilentlyContinue) -ne $null) {
	nuget restore
}
else {
	"Nuget was not found and is required to run bootstrap.ps. Download and retry now?"
	choice /c yn
	if ($LASTEXITCODE -eq 1) {
		"Downloading nuget..."
		$nugetLocation = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
		(New-Object Net.WebClient).DownloadFile($nugetLocation, "$PSScriptRoot\nuget.exe")
		Invoke-Expression "./nuget restore"
	}
}﻿
 
 function FetchLib($name) {
	"Fetching $name..."
	$url = "https://libs.hearthsim.net/hdt/$name.dll"
	try { (New-Object Net.WebClient).DownloadFile($url, "$PSScriptRoot\lib\$name.dll") }
    catch { $error[0].Exception.ToString() }
}

md -Force $PSScriptRoot\lib | Out-Null

FetchLib "HearthDb"
FetchLib "HearthMirror"
