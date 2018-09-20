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
      //Load the assembly from the given filepath
      var contextAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(args[0]);

      //Make an DbContextOptionsBuilder of the given context type
      var contextType = contextAssembly.GetType(args[1]);
      var optionsType = typeof(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<>);
      var dbOptions = optionsType.MakeGenericType(contextType);
      var dbOptionsBuilder = Activator.CreateInstance(dbOptions);

      var useSqlMethods = typeof(SqlServerDbContextOptionsExtensions).GetMethods().Where(m => m.Name == "UseSqlServer").ToArray();

      var useSql = useSqlMethods[2];
      object[] methodParams = { dbOptionsBuilder, "Server=iqi_reports_test_db,1433;Database=InqwizitReports;User ID=sa;Password=yourStrong(!)Password;MultipleActiveResultSets=true", null };

      useSql = useSql.MakeGenericMethod(contextType);
      useSql.Invoke(null, methodParams);

      var dbContextOptions = dbOptionsBuilder.GetType().GetProperties().Where(p => p.Name == "Options").First().GetValue(dbOptionsBuilder);

      object[] contextParams = { dbContextOptions };

      var constructor = contextType.GetConstructors()[0];
      var thing = (DbContext)constructor.Invoke(contextParams);
      thing.Database.Migrate();
      Console.WriteLine("Hello World!");
    }
  }
}
