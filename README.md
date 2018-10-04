# Core-Migrator
Migrates EF Core DBContexts in a standalone console application, without the need to include the DotNet Core SDK in a cloud image.

Currently only supports mssql database contexts

Usage: `dotnet run /app/out/dllwithcontext.dll Application.Namespace.ApplicationContext Server=mssqlServer;Database=MyDb;User Id=Foo;Password=Bar`
