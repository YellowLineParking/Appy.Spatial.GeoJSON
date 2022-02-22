$invokeBuildVersion = '5.7.2';
$newtonsoftJsonVersion = '1.0.2.201';
$yamlDotNetVersion = '9.1.4';
$octopusToolsVersion = '7.4.3136';

Write-Host 'Restoring NuGet packages';
& dotnet restore

Write-Host "Installing YamlDotNet $yamelDotNetVersion Nuget Package";
nuget install YamlDotNet -Version $yamlDotNetVersion -OutputDirectory "packages";

Write-Host "Installing Newtonsoft.Json $newtonsoftJsonVersion PS Module";
Install-Module -Name newtonsoft.json -RequiredVersion $newtonsoftJsonVersion -Scope CurrentUser;

Write-Host "Installing Invoke-Build $invokeBuildVersion PS Module";
Install-Module -Name InvokeBuild -RequiredVersion $invokeBuildVersion -Scope CurrentUser;

. '.\functions.ps1' -yamelDotNetVersion $yamlDotNetVersion -octopusToolsVersion $octopusToolsVersion -artifactsPath artifacts;

$allArgs = $PsBoundParameters.Values + $args

& Invoke-Build $allArgs -File tasks.ps1;