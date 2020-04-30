using System;
using System.Threading.Tasks;
using EFCoreDemos.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace EFCoreDemos
{
   class Program
   {
      private static bool _activateLazyLoading = false;

      static async Task Main()
      {
         var serviceProvider = BuildServiceProvider();
         await EnsureDatabaseAsync(serviceProvider);

         using var scope = serviceProvider.CreateScope();

         var demos = scope.ServiceProvider.GetRequiredService<Demos>();
         demos.Execute();
      }

      private static async Task EnsureDatabaseAsync(IServiceProvider serviceProvider)
      {
         using var scope = serviceProvider.CreateScope();

         var ctx = scope.ServiceProvider.GetRequiredService<DemoDbContext>();
         await ctx.Database.EnsureCreatedAsync();
      }

      private static IServiceProvider BuildServiceProvider()
      {
         var config = GetConfiguration();
         var connString = config.GetConnectionString(nameof(DemoDbContext))
                          ?? throw new Exception($"No connection string for '{nameof(DemoDbContext)}' provided");

         var services = new ServiceCollection()
                        .AddSingleton(config)
                        .AddLogging(builder =>
                        {
                           var serilog = new LoggerConfiguration()
                                         .ReadFrom.Configuration(config)
                                         .CreateLogger();

                           builder.AddSerilog(serilog);
                        })
                        .AddScoped<Demos>()
                        .AddDbContext<DemoDbContext>((serviceProvider, builder) => builder.UseSqlServer(connString)
                                                                                          .UseLazyLoadingProxies(_activateLazyLoading)
                                                                                          .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
                                                                                          .EnableDetailedErrors()
                                                                                          .EnableSensitiveDataLogging());


         return services.BuildServiceProvider();
      }

      private static IConfiguration GetConfiguration()
      {
         return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
      }
   }
}
