# DifBot

## About
Source code for the Discord Internet Forum Bot

## Local developement

### Requirements

* [.NET 8 SDK](https://dotnet.microsoft.com/download/visual-studio-sdks)
* [Visual Studio](https://visualstudio.microsoft.com/) (or any other preferred editor + dotnet command line tool)
* [PostgreSQL 12+](https://www.postgresql.org/)

### Build from Visual Studio

Build solution

### Build from dotnet command line tool

`dotnet build`

### Connection string format
`Server=_DB_SERVER_IP_;Database=_DB_NAME_;User Id=_DB_USER_;Password=_DB_PASSWORD_;`

### Secrets
Connection string 

`"DifBot:ConnectionString" "CONN_STRING_GOES_HERE"​`

Bot token

`"DifBot:BotToken" "TOKEN_GOES_HERE"​`

## Credits

* [DSharp+](https://github.com/DSharpPlus/DSharpPlus) .net discord wrapper
* [MediatR](https://github.com/jbogard/MediatR) used in CQRS pattern implementation
* [Npgsql](https://github.com/npgsql/npgsql) .net data provider for PostgreSQL
