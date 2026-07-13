# Common library
Library with published contract between Inventory service and other services

## Building app
dotnet build

## Specify dotnet local Nuget Package source path (you need to do it only once)
dotnet nuget add source "<Absolute_path_to_package_folder>" -n PlayEconomy

## Pack library and export to output folder
dotnet pack -o ../../../packages/
dotnet pack -o ../../../packages/ -p PackageVersion=1.0.1

## Publish package to Github
```powershell
$version="1.0.1"
$owner="Play-Microservices"
$gh_pat="[PAT HERE]"

dotnet pack src/Play.Inventory.Contracts/ --configuration Release -p:PackageVersion=$version -p:RepositoryUrl=http://github.com/$owner/play.inventory -o ../packages

dotnet nuget push ../packages/Play.Inventory.Contracts.$version.nupkg --api-key $gh_pat --source "github"
```