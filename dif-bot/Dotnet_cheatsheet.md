# Dotnet cheatsheet

## Installation

### Install ef-tools
`dotnet tool install dotnet-ef -g`

## Database

### Add migration
`dotnet ef migrations add MigrationName -p DifBot.Data.Migrations`

### Remove last migration
`dotnet ef migrations remove -p DifBot.Data.Migrations`

### Update dev database
`dotnet ef database update -p DifBot.Data.Migrations`

### Update to target migration
`dotnet ef database update TargetMigration -p DifBot.Data.Migrations`

### Generate database upgrade script
`dotnet ef migrations script --idempotent -o database_migration.sql -p DifBot.Data.Migrations`

## Publish

### Publish windows release
`dotnet publish -c Release -r win10-x64 --self-contained false`

### Publish linux release
`dotnet publish -c Release -r linux-x64 --self-contained false`

## User secrets

### List secrets
`dotnet user-secrets list`

### Set secrets for local development
`dotnet user-secrets set "SECRET_NAME" "SECRET_VALUE"​`
