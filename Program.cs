using System;
using System.Linq;
using System.Runtime.Loader;
using Microsoft.EntityFrameworkCore;

namespace Core_Migrator
{
  class Program
  {
    static void Main(string[] args)
    {
      if(args.Length != 3) 
      {
        Console.WriteLine("Please supply all 3 required arguments: ");
        Console.WriteLine("Ex. dotnet run /app/out/dllwithcontext.dll Application.Namespace.ApplicationContext Server=mssqlServer;Database=MyDb;User Id=Foo;Password=Bar");
        return;
      }

      string fullAssemblyPath = args[0];
      string dbContextName = args[1];
      string dbConnectionString = args[2];

      //Load the assembly from the given filepath
      Console.WriteLine("Loading assembly...");
      AssemblyResolver ar = new AssemblyResolver(fullAssemblyPath);
      var contextAssembly = ar.Assembly;

      //Get the DbContext of the given name
      Console.WriteLine("Getting DbContext...");
      var contextType = contextAssembly.GetType(dbContextName);

      //Create a DbContextOptionsBuilder of the type
      Console.WriteLine("Creating DbContextOptionsBuilder...");
      var optionsType = typeof(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<>);
      var dbOptions = optionsType.MakeGenericType(contextType);
      var dbOptionsBuilder = Activator.CreateInstance(dbOptions);

      //Configure the DbContextOptionsBuilder connection
      Console.WriteLine("Configuring DbContextOptionsBuilder...");
      var useSqlMethods = typeof(SqlServerDbContextOptionsExtensions).GetMethods().Where(m => m.Name == "UseSqlServer").ToArray();
      var useSql = useSqlMethods[2];
      object[] methodParams = { dbOptionsBuilder, dbConnectionString, null };

      useSql = useSql.MakeGenericMethod(contextType);
      useSql.Invoke(null, methodParams);

      //Get The DbContextOptions from the builder
      var dbContextOptions = dbOptionsBuilder.GetType().GetProperties().Where(p => p.Name == "Options").First().GetValue(dbOptionsBuilder);

      //Attempt to grab a dbcontext constructor with one DbContextOptions parameter
      var constructor = contextType.GetConstructors()
                                    .Where(c => c.GetParameters()
                                      .Any(p => p.ParameterType.BaseType.Equals(typeof(DbContextOptions)) && c.GetParameters().Length == 1))
                                    .First();

      //Create the DbContext
      object[] contextParams = { dbContextOptions };
      var dbContext = (DbContext)constructor.Invoke(contextParams);

      //Migrate that sucker
      Console.WriteLine("Migrating Db...");
      dbContext.Database.Migrate();
      Console.WriteLine("Finished Migrating.");
    }
  }
}
