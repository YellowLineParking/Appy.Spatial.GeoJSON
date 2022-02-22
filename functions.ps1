#region Variables

$script:artifactsPath = "artifacts"
$script:prereleaseNumber = ""
$script:buildTools = New-Object BuildTools
$workingDir = (Get-Location).Path
Add-Type -AssemblyName System.Web
$yamlPackagePath = $buildTools.GetPackagePath("YamlDotNet");
Add-Type -Path "$yamlPackagePath\lib\netstandard1.3\YamlDotNet.dll";
$script:projectConfig = $null

Import-Module Newtonsoft.Json

Write-Host "WorkingDir: $workingDir";

#endregion

#region General

function Get-ProjectsForTask($task){
	$task = $task.ToLower()

	$projectConfig = $buildTools.GetProjectConfig()
    if($projectConfig -eq $null) {
        Write-Warning "The config.yml file contains no configuration";
        return;
    }
	
	$config = @{
		"clean" = "App", "XUnit", "Package", "AppPackage";
		"compile" = "App", "XUnit", "Package", "AppPackage";
		"test" = "XUnit";
		"pack" = "Package", "App", "AppPackage";
		"push" = "Package", "App", "AppPackage";
		"release" = "App";
		"deploy" = "App";
	}
	$projectTypes = $config[$task]

    return $projectConfig.Keys | 
        Where { 
			($projectTypes -contains $projectConfig[$_]["Type"] -and `
			($projectConfig[$_]["Exclude"] -eq $null -or $projectConfig[$_]["Exclude"].ToLower().IndexOf($task) -eq -1) -or `
			 ($projectConfig[$_]["Include"] -ne $null -and $projectConfig[$_]["Include"].ToLower().IndexOf($task) -ne -1))
		} | 
        ForEach-Object { 
            @{
                "Name" = $_;
                "Type" = $projectConfig[$_]["Type"];
                "Config" = $projectConfig[$_]["Config"];
            }}
}
function Get-PackagePath($packageId, $project) {
    return $buildTools.GetPackagePath($packageId, $project);
}#endregion

#region "Clean"

function Clean-Folder($folder){
	"Cleaning $folder"
	Remove-Item $folder -Recurse -Force -ErrorAction SilentlyContinue | Out-Null
}

#endregion

#region Compile

function Compile-Project($projectName) {
	$projectConfig = $buildTools.GetProjectConfig($projectName)
	$version = Get-Version $projectName
    $framework = $buildTools.GetBuildVersion($projectName);
    	
    if ($framework.UseDotNetCli){
		Compile-DotNetCli $projectName $artifactsPath $config $version
    }
}

function Compile-DotNetCli($projectName, $artifactsPath, $config, $version) {
    $projectFile = $buildTools.GetProjectFilePath($projectName)
	$projectConfig = $buildTools.GetProjectConfig($projectName)
		
	Write-Host "Compiling Dotnet $projectName"		
    exec { dotnet build $projectFile --configuration $config --no-restore  }
	
	$isDotnetExeProject = (Select-String -pattern "<OutputType>Exe</OutputType>" -path $projectFile) -ne $null -and (Select-String -pattern "<PackAsTool>true</PackAsTool>" -path $projectFile) -eq $null
	$isDotnetWebProject = $projectConfig["Type"] -eq "App" -and !$isDotnetExeProject
	
	if ($isDotnetWebProject -or $isDotnetExeProject) {
		Write-Host "Publishing Dotnet $projectName to $artifactsPath"
		Write-Host "exec { dotnet publish $projectFile --no-build --no-restore --configuration $config --output $workingDir\$artifactsPath\$projectName --version-suffix $version }"
		$output = "$workingDir\$artifactsPath\$projectName"
		exec { dotnet publish $projectFile --no-build --no-restore --configuration $config --output $output --version-suffix $version }
		
		if($projectConfig["Type"] -eq "App") {
			Compile-ApiSpecs $projectName $output
		}

		return
	}

	$singleFramework = Get-Content $projectFile | Where { $_.Contains("<TargetFramework>") }
	$multiFrameworks = Get-Content $projectFile | Where { $_.Contains("<TargetFrameworks>") }
	$frameworks = If ($singleFramework -ne $null) { @(([xml]$singleFramework).TargetFramework) } Else { ([xml]$multiFrameworks).TargetFrameworks.Split(';') }
	$frameworks | ForEach-Object {
		$framework = $_		
		$output = "$workingDir\$artifactsPath\$projectName\$framework"
		Write-Host "Publishing Dotnet $projectName with framework $framework to $workingDir\$artifactsPath"		
		exec { dotnet publish $projectFile --no-build --no-restore --configuration $config --output $output --framework $framework --version-suffix $version }
	}
}


function NormaliseName($name) {
	return $name -replace "[^\w]",""
}

function GetVersionFromFileName($fileName) {
	if(!($fileName -match "(?<version>v\d+(?:_\d+)?)")) {
		return $null;
	}

	return NormaliseVersion -version $Matches.version
}

function NormaliseVersion([string] $version) {
	# This function normalises a version string to a proper v{major}.{minor} format
    if("" -eq $version -or $null -eq $version) {
        return $null;
	}
	
	$version = $version.Replace("_", ".")

    if(!($version -match "^\d+.\d+$")) {
        if($version -match "^[vV]?\d+.\d+.*") {
            return $version -replace "^[vV]?(\d+.\d+).*",'v$1'
        } elseif($version -match "^[vV]?\d+$") {
            return $version -replace "^[vV]?(\d+)$",'v$1.0'
        } else {
            throw "Api Version $($version) was in an unexpected format. Version should be in semver format."
        }
    }
    return $version
}

function NormaliseFileVersionStamp($version) {
	# This function takes a version number in v{major}.{minor} format and converts it so
	# it can be embedded in a file name.

	if("" -eq $version -or $null -eq $version) {
        return "";
    }

	# Replace dots with undescores
	$version = $version.Replace(".", "_")
	# Trim off zeros for the minor version
	$version = $version -replace "^(v\d+)(_0)?$", '$1'
	return $version
}

#
# Generates a MD5 hash of a string
#
function Get-StringHash([string] $string) 
{ 
	$StringBuilder = New-Object System.Text.StringBuilder 
	[System.Security.Cryptography.HashAlgorithm]::Create("MD5").ComputeHash([System.Text.Encoding]::UTF8.GetBytes($String))|%{ 
		[Void]$StringBuilder.Append($_.ToString("x2")) 
	} 
	$StringBuilder.ToString() 
}

function Compile-GetApiName($projectName) {
	return NormaliseName $projectName;
}

function Compile-QualifyApiSpecFileName($projectName, $version, $extension) {
	$apiName = Compile-GetApiName $projectName
	$version = NormaliseFileVersionStamp $version
	
	return "$apiName-$(NormaliseFileVersionStamp $version).$extension"
}

#region Tests

function Execute-Xunit($projectName) {
    $frameworkVersion = $buildTools.GetBuildVersion($projectName);
    	
    if ($frameworkVersion.UseDotNetCli){
		Execute-Xunit-DotNetCli $projectName $artifactsPath $config
    }
}

function Execute-Xunit-DotNetCli($projectName, $artifactsPath, $config) {
    $projectFile = $buildTools.GetProjectFilePath($projectName)
	$singleFramework = Get-Content $projectFile | Where { $_.Contains("<TargetFramework>") }
	$multiFrameworks = Get-Content $projectFile | Where { $_.Contains("<TargetFrameworks>") }
	$frameworks = If ($singleFramework -ne $null) { @(([xml]$singleFramework).TargetFramework) } Else { ([xml]$multiFrameworks).TargetFrameworks.Split(';') }

	$frameworks | ForEach-Object {
		$framework = $_		
		Write-Host "Executing tests for $projectName with framework $framework to $artifactsPath"		
		exec { dotnet test $projectFile --configuration $config --no-build --no-restore --framework $framework --test-adapter-path:. "--logger:xunit;LogFilePath=$workingDir/$artifactsPath/xunit-$projectName-$framework.xml" --verbosity quiet }
	}
}

#endregion

#region Push and Pack
$versions = @{}
function Get-VersionFromAssemblyInfo($path){
    if (!(Test-Path $path)) {
        return
    }
    $line = Get-Content $path | Where { $_.Contains("AssemblyVersion") }
    if ($line){
        return $line.Split('"')[1]
    }
    return
}
function Get-VersionFromCsProj($path){
    if (!(Test-Path $path)) {
        return
    }
    $line = Get-Content $path | Where { $_.Contains("<Version>") }
    if ($line){
        return ([xml]$line).Version
    }
    return
}

function Get-Version($projectName, [switch]$includeBuildNumber) {
    $version = Get-VersionFromCsProj $buildTools.GetProjectFilePath($projectName)
    
	if (!$version){
        $version = Get-VersionFromCsProj "$projectName\..\version.props"
    }
    if (!$version) {
		$version = "1.0.0"
	}

	# We no longer want to replace on wildcard, since dotnet core no longer supports wildcard auto versioning. Replace the patch number with
	# 0 and we'll autoincrement the patch number using regex.
	$version = $version.Replace("*", "0")

	# Ensure the version number is in the Major.Minor.Patch format, and if it isn't, fix it.
	if(!($version -match "\d+\.\d+\.\d+")) {
		if($version -match "\d+\.\d+") {
			$version = "$version.0"
		} elseif ($version -match "\d+") {
			$version = "$version.0.0"
		} else {
			Write-Error "Version number $version is in an unexpected format for project $projectName"
		}
	}

	$branch = Get-CurrentBranch
	$isGeneralRelease = Is-GeneralRelease $branch
	$isLocalBuild = Is-LocalBuild

	# If this build is a pre-release build (i.e. it's a build on a feature branch) then append the branch name to the version
	if ($isLocalBuild -or !$isGeneralRelease){
        if ([String]::IsNullOrEmpty($script:prereleaseNumber)) {
            $script:prereleaseNumber = Get-PrereleaseNumber $branch;
        }

        $version = "$version-$script:prereleaseNumber"
	} 
	
	if($includeBuildNumber) {
		$buildNumber = $env:APPVEYOR_BUILD_NUMBER;

		if($null -eq $buildNumber) {
			$buildNumber = "0";
		}
			
		# Replace the patch version number with the appveyor build number
		$version = $version -replace "(\d+\.\d+\.)(\d+)",('${1}' + $buildNumber)
	}
		
	Write-Host "Assigning version stamp $version"

	return $version
}

function Get-CurrentBranch{
	if ([String]::IsNullOrEmpty($env:APPVEYOR_REPO_BRANCH)){
		$branch = git branch | Where {$_ -match "^\*(.*)"} | Select-Object -First 1
	} else {
		$branch = $env:APPVEYOR_REPO_BRANCH
	}
	return $branch
}

function Is-GeneralRelease($branch){
	return ($branch -eq "develop" -or $branch -eq "master")
}

function Is-LocalBuild(){
	return [String]::IsNullOrEmpty($env:APPVEYOR_REPO_BRANCH)
}

function Get-PrereleaseNumber($branch){
	$branch = $branch.Replace("* ", "")
    if ($branch.IndexOf("/") -ne -1){
        $prefix = $branch.Substring($branch.IndexOf("/") + 1)
    } else {
        $prefix = $branch
    }

    $prefix = $prefix.Substring(0, [System.Math]::Min(30, $prefix.Length))
	return $prefix + "-" + $(Get-Date).ToString("yyMMddHHmmss") -Replace "[^a-zA-Z0-9-]", ""
}

function Pack-Project($projectName){
	$version = Get-Version $projectName
    $frameworkVersion = $buildTools.GetBuildVersion($projectName);

    if ($frameworkVersion.UseDotNetCli) {
        Pack-DotNetCli $projectName $version
    }
}

function Pack-DotNetCli($project, $version) {		
    $projectFilePath = $buildTools.GetProjectFilePath($project)
	Write-Host "exec { dotnet pack $projectFilePath --configuration $config --no-restore --output $workingDir\$artifactsPath -p:Version=$version }"
	exec { dotnet pack $projectFilePath --configuration $config --no-restore --output $workingDir\$artifactsPath -p:Version=$version }  
}

function Push-Package($package, $nugetPackageSource, $nugetPackageSourceApiKey, $ignoreNugetPushErrors) {
	$package = $package -replace "\.", "\."
	$package = @(Get-ChildItem "$artifactsPath\*.nupkg") | Where-Object {$_.Name -match "$package\.\d*\.\d*\.\d*.*\.nupkg"}
	
	try {
		Write-Host "NuGet push $package -Source $nugetPackageSource -ApiKey $nugetPackageSourceApiKey 2>&1"
		if (![string]::IsNullOrEmpty($nugetPackageSourceApiKey) -and $nugetPackageSourceApiKey -ne "LoadFromNuGetConfig") {
			$out = nuget push $package -Source $nugetPackageSource -ApiKey $nugetPackageSourceApiKey 2>&1
		}
		else {
			$out = nuget push $package -Source $nugetPackageSource 2>&1
		}

		Write-Host $out
		
	}
	catch {
		$errorMessage = $_
		$ignoreNugetPushErrors.Split(";") | foreach {
			if ($([String]$errorMessage).Contains($_)) {
				$isNugetPushError = $true
			}
		}
		if (!$isNugetPushError) {
			throw
		}
		else {
			Write-Host "WARNING: $errorMessage"
		}
	}
}

#endregion

#region Build Tools

function ConvertFrom-Yaml(
	[parameter(
        Mandatory         = $true,
        ValueFromPipeline = $true)]
	$yaml) {
  	if([string]::IsNullOrWhiteSpace($yaml)) {
		return $null;
	}
	$yaml = $yaml.Replace("`t", "    ")
	$stringReader = New-Object System.IO.StringReader([string]$yaml)
	$Deserializer = New-Object -TypeName YamlDotNet.Serialization.Deserializer
	return $Deserializer.Deserialize([System.IO.TextReader]$stringReader)
}

function ConvertTo-Yaml(
	[parameter(
        Mandatory         = $true,
        ValueFromPipeline = $true)]
	$object) {
	$SerializerBuilder = New-Object -TypeName YamlDotNet.Serialization.SerializerBuilder
	$SerializerBuilder = $SerializerBuilder.WithMaximumRecursion(99)
	$Serializer = $SerializerBuilder.Build()
	return $Serializer.Serialize($object);
}

class BuildTools {
	[object]$projectConfig = $null;
    #
    # Searches a number of locations on the host machine for the specified nuget package
    #
    [string] GetPackagePath($packageId) {
        return $this.GetPackagePath($packageId, $null)
    }

    #
    # Searches a number of locations on the host machine for the specified nuget package
    #
    [string] GetPackagePath($packageId, $project) {
        [BuildVersion]$package = $null;

        if($project -ne $null) {
            $framework = $this.GetBuildVersion($project);
                       
            if($framework.UseDotNetCli -and (Test-Path "$project\$project.csproj")) {
                [xml]$packagesXml = Get-Content "$project\$project.csproj";
                $packageXml = $packagesXml.Project.ItemGroup.PackageReference | Where { $_.Include -eq $packageId }
                if($packageXml -ne $null) {
                    $package = New-Object BuildVersion -Property @{ Id = $packageXml.Include; Version = $packageXml.Version }
                }
            }
        } else {
            $package = New-Object BuildVersion -Property @{ Id = $packageId; Version = "*" }
        }

        if($package -ne $null) {
            # First, check the local package cache
            $children = Get-ChildItem "packages" -Filter $package.GetPackageFolderName();

            if($children -eq $null) {
                #Then, check the global package cache
				$packagePath = "$env:HOME"				
                $children = Get-ChildItem "$packagePath\.nuget\packages\$($package.Id)" -Filter $package.Version
            }

            if($children -ne $null)
            {
                [System.IO.DirectoryInfo]$packageDirectory = $null;

                if($children -is [system.array]) {
                    $packageDirectory = $children[0];
                }
                else {
                    $packageDirectory = $children;
                }

                return $packageDirectory.FullName;
            }
        }
        
        if($project -ne $null) {
            throw "$packageId is required in $project but it is not installed. Please install $packageId in $project."
        }

        throw "$packageId is required but it is not installed. Please install $packageId."
    }

    [FrameworkVersion]GetBuildVersion([string]$project) {
        $projectFilePath = $this.GetProjectFilePath($project)
        $dotNetCoreFrameworkMatch = (Select-String -pattern '<TargetFramework[s]?>(.+)<\/TargetFramework[s]?>' -path $projectFilePath)
     
        if($dotNetCoreFrameworkMatch -ne $null) {
            return New-Object FrameworkVersion -Property @{ 
                Version = $dotNetCoreFrameworkMatch.Matches[0].Groups[1].Value; 
                UseDotNetCli = $true;
            };
        }

        throw "Unable to determine version of project";
    }

    [string]GetProjectFilePath([string]$project) {
        return "$project/$project.csproj"
    }

	[object]GetProjectConfig() {
		if($null -eq $this.projectConfig) {
			$this.projectConfig = Get-Content -Path ".\config.yml" -Raw | ConvertFrom-Yaml
		}

		return $this.projectConfig;
	}

	[object]GetProjectConfig($projectName) {
		$conf = $this.GetProjectConfig();

		if($null -ne $conf) {
			return $conf[$projectName]
		}

		return $null;
	}
}

class BuildVersion {
    [string]$Id
    [string]$Version
    
    [string] GetPackageFolderName() {
        return $this.Id + "." + $this.Version;
    }
}

class FrameworkVersion {
    [string]$Version = $null;
    [bool]$UseDotNetCli = $false;
}

#endregion