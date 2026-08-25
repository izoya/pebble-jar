# Setup dotnet tools
From the repository root, install dotnet-ef as a project-local tool once:
```shell
dotnet new tool-manifest
dotnet tool install dotnet-ef --version 10.0.11
```

## Add migration
```shell
dotnet ef migrations add AddFinancialInstitutions --project src/PebbleJar.Infrastructure --startup-project src/PebbleJar.Api --output-dir Data/Migrations
```
## Update DB
```shell
dotnet ef database update --project src/PebbleJar.Infrastructure --startup-project src/PebbleJar.Api
```
## Restore DB
```shell
dotnet tool restore
```
