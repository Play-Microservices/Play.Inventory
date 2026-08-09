# Play.Inventory

## Inventory Service

Service to list and manage player inventory.

### Building app

```bash
dotnet build
```

### Running app

```bash
dotnet run
```

### Running MongoDB with localhost volume

```bash
docker run -d --rm --name mongo -p 27017:27017 -v mongodbdata:/data/db mongo
```

### Add reference to exported Common library

```bash
dotnet add package Play.Common
```

### RabbitMQ default credentials

```text
guest:guest
```

### Build the docker image

```powershell
$env:GH_OWNER="Play-Microservices"
$env:GH_USERNAME="[USERNAME HERE]"
$env:GH_PAT="[PAT HERE]"
$version="1.0.2"
docker build --secret id=GH_USERNAME --secret id=GH_OWNER --secret id=GH_PAT -t play.inventory:$version .
```

```bash
export GH_OWNER="Play-Microservices"
export GH_USERNAME="[USERNAME HERE]"
export GH_PAT="[PAT HERE]"
version="1.0.2"

docker build \
  --secret id=GH_OWNER \
  --secret id=GH_USERNAME \
  --secret id=GH_PAT \
  -t play.inventory:$version .
```

### Run the docker image

```powershell
version="1.0.2"
docker run -it -=rm -p 5004:8080 --name catalog -e MongoDbSettings__Host=mongo -e RabbitMQSettings__Host=rabbitmq -e --network playinfrastructure_default play.inventory:$version
```

```bash
version="1.0.2"
docker run -it --rm \
  -p 5004:8080 \
  --name catalog \
  -e MongoDbSettings__Host=mongo \
  -e RabbitMQSettings__Host=rabbitmq \
  --network playinfrastructure_default \
  "play.inventory:$version"
```

---

## Contract Library

Library with published contracts between the Inventory service and other services.

### Building app

```bash
dotnet build
```

### Specify dotnet local NuGet package source path

You only need to do this once.

```bash
dotnet nuget add source "<Absolute_path_to_package_folder>" -n PlayEconomy
```

### Pack library and export to output folder

```bash
dotnet pack -o ../../../packages/
dotnet pack -o ../../../packages/ -p:PackageVersion=1.0.1
```

### Publish package to GitHub

```bash
version="1.0.1"
owner="Play-Microservices"
gh_pat="[PAT HERE]"

dotnet pack src/Play.Inventory.Contracts/ \
  --configuration Release \
  -p:PackageVersion=$version \
  -p:RepositoryUrl=https://github.com/$owner/play.inventory \
  -o ../packages

dotnet nuget push \
  ../packages/Play.Inventory.Contracts.$version.nupkg \
  --api-key $gh_pat \
  --source "github"
```
