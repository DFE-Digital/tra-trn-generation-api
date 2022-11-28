# TRN Generation API

## Overview

A service that facilitates the generation of unique TRNs (Teacher Reference Numbers) via a REST API.

## Setup

### Developer setup

The API is an ASP.NET Core 7 web application. To develop locally you will need the following installed:
- Visual Studio 2022 (or the .NET 7 SDK and an alternative IDE/editor);
- a local PostgreSQL 13+ instance;

### Initial setup

#### User Secrets

Install PostgreSQL then add a connection string to user secrets for the `TrnGeneratorApi` and `TrnGeneratorApi.IntegrationTests` projects.

```shell
dotnet user-secrets --id TrnGeneratorApi set ConnectionStrings:DefaultConnection "Host=localhost;Username=your_postgres_user;Password=your_postgres_password;Database=trn_generator"
dotnet user-secrets --id TrnGeneratorApi.IntegrationTests set ConnectionStrings:DefaultConnection "Host=localhost;Username=your_postgres_user;Password=your_postgres_password;Database=trn_generator_tests"
```
Where `your_postgres_user` and `your_postgres_password` are the username and password of your Postgres installation, respectively.

Next set the API Key(s) you want to use to authenticate/authorize calls to the API for local development.

```shell
dotnet user-secrets --id TrnGeneratorApi set ApiKeys:0 "your_API_Key"
```

Where `your_API_Key` will be used in the `Authorization` header in calls to the API e.g. `Bearer your_API_Key`

#### Database setup

To create the initial database you need to apply the Entity Framework migrations.

You can do this using the Package Manager Console in Visual Studio or using the .NET Core CLI.

##### Package Manager Console

In Visual Studio, launch the Package Manager Console from the `Tools -> NuGet Package Manager -> Package Manager Console` menu option.

In the Package Manager Console ensure that the `Default Project` option is set to `src\TrnGeneratorApi`.

At the prompt execute the `Update-Database` command:

```
PM> Update-Database
```

Launch `pgAdmin` and verify that the database has been created in PostgreSQL.

###### .NET Core CLI

Ensure that the Entity Framework .NET Core CLI tools are installed as detailed [here](https://learn.microsoft.com/en-us/ef/core/cli/dotnet#installing-the-tools).

Launch a commandline and set the current directory to the `src\TrnGeneratorApi` directory containing the .NET core project (i.e. which contains the EF migrations).

At the prompt execute the following:

```
dotnet ef database update
```

Launch `pgAdmin` and verify that the database has been created in PostgreSQL.
