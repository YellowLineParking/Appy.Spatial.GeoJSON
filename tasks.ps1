$script:config = "Release"

task Clean {
	Clean-Folder $artifactsPath
	Clean-Folder TestResults
	Get-ProjectsForTask "Clean" | ForEach-Object {
		Clean-Folder "$($_.Name)\bin"
	}
	New-Item $artifactsPath -Type directory -Force | Out-Null
}

task Compile {
	Get-ProjectsForTask "Compile" | ForEach-Object {
		Compile-Project $_.Name
	}
}

task Test {
	Get-ProjectsForTask "Test" | 
		Where { $_.Type -eq "XUnit"} |
		ForEach-Object {
			Execute-XUnit $_.Name
		}
}

task Pack {
	Get-ProjectsForTask "Pack" | ForEach-Object {
		if ($_.Type -eq "Package") {
			Pack-Project $_.Name			
		}
	}
}

task Push {
	Get-ProjectsForTask "Push" | 
		Where { $_.Type -eq "Package" -Or $_.Type -eq "AppPackage"} |
		ForEach-Object { 
			Push-Package $_.Name $env:ylp_nugetPackageSource $env:ylp_nugetPackageSourceApiKey "409"
		}
}

task dev Clean, Compile, Test, Pack
task ci dev, Push