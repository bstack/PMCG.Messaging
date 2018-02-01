# Stop on error, see http://redsymbol.net/articles/unofficial-bash-strict-mode/
$ErrorActionPreference = 'Stop'

### Args
if ($args.Count -ne 1) { write-host "Usage is $($MyInvocation.MyCommand.Path) version"; exit 1 }
# Version
[string]$version = $args[0]

write-host "### Paths"
$rootDirectoryPath = (split-path ($MyInvocation.MyCommand.Path))
$solutionFilePath = join-path $rootDirectoryPath PMCG.Messaging.sln
$packagesDirectoryPath = join-path $rootDirectoryPath packages


write-host "### Empty packages directory"
if (test-path $packagesDirectoryPath) { Remove-Item -recurse -force $packagesDirectoryPath }

write-host "### Build and pack"
# We could have just done a build and pack in 1 go with dotnet pack without the --no-build option, but better to keep seperate
dotnet clean $solutionFilePath
dotnet build $solutionFilePath --configuration Release /p:PackageVersion=$version
dotnet pack $solutionFilePath --no-build --configuration Release /p:PackageVersion=$version --output $packagesDirectoryPath
if ($LastExitCode -ne 0) { write-host 'Build and pack failure !'; exit 1 }

write-host "### Run tests"
# Test all release UT assemblies within the test directory
# Note on output. Tests DO run, nunit runner outputs an exception after tests ran. This can be ignored
#      Exception in Unloading AppDomain. This is an NUnit issue. See https://github.com/nunit/nunit-console/issues/191
Get-ChildItem test -recurse -include *.UT.dll | ? { $_.FullName.IndexOf('bin\Release') -gt -1 } | % {
	# Using vstest as we want to run tests against the built dll, other option is to use dotnet test against csproj
	dotnet vstest $_.FullName
	if ($LastExitCode -ne 0) { write-host 'Run tests failure !'; exit 1 }
}

write-host "### Push instruction"
write-host "Copy-Item $packagesDirectoryPath\PMCG.Messaging.*.nupkg' -Exclude '*.AT.*' YOUR_NUGET_LOCATION"