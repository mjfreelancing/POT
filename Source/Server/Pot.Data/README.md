# Pot.Data

This project manages Entity Framework Core database migrations and contains the required design-time tools for the POT application.

## Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- Access to the target database (connection string configured in the main application or via environment variables)

## Generating a New Migration

### Using the .NET CLI

1. Open a terminal in the `Pot.Data` project directory.
2. Run the following command, replacing `MigrationName` with a descriptive name for your migration:
`dotnet ef migrations add MigrationName --project Pot.Data --startup-project Pot.Data.Migrations`

- `--project` specifies the migrations project (Pot.Data).
- `--startup-project` should be set to Pot.Data.Migrations.

### Using Visual Studio (Package Manager Console)

1. Open the solution in Visual Studio.
2. Go to **Tools > NuGet Package Manager > Package Manager Console**.
3. In the Package Manager Console (PMC), ensure the **Default project** is set to `Pot.Data`.
4. Run the following command, replacing `MigrationName` with your migration name:
`Add-Migration MigrationName -StartupProject Pot.Data.Migrations`

## Applying Migrations to the Database

### Using the .NET CLI

1. Ensure your connection string is set correctly (in `appsettings.json` or via environment variables).
2. Run the following command to apply all pending migrations:
dotnet ef database update --project Pot.Data --startup-project Pot.Data
### Using Visual Studio (Package Manager Console)

1. Open the solution in Visual Studio.
2. Go to **Tools > NuGet Package Manager > Package Manager Console**.
3. In the Package Manager Console, ensure the **Default project** is set to `Pot.Data`.
4. Run the following command:
`Update-Database -StartupProject Pot.Data.Migrations`

### Using the Pot.Data.Migrations Application

You can also apply migrations by running the `Pot.Data.Migrations` application directly. This is useful for automated deployments or when you prefer not to use the CLI or Visual Studio.

1. Build the solution.
2. Run the `Pot.Data.Migrations` project using your preferred method (e.g., Visual Studio, `dotnet run`, or as a published executable):
dotnet run --project Pot.Data.Migrations
This will apply all pending migrations to the database using the connection string configured for `Pot.Data.Migrations`.

## Additional Resources
- [EF Core Migrations Documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)

---

For troubleshooting or advanced scenarios, refer to the official EF Core documentation or the POT project documentation.
