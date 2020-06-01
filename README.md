# Database Logger

***ILogger interface implementation for usage of EntityFrameworkCore database context as a backend.***

This logger allows to store logs in memory by using collector object that makes logging very fast.
When this logger is enabled its background service synchronizing all the logs to database asynchronously.

### Usage 

##### Enabling logger
```CSharp
private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureLogging(builder => builder
            .AddDatabase<AppDbContext>()
        )
        .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
```

##### Enabling with configuration
*It is better to use appsettings.json file to avoid hardcoding.*
```CSharp
private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureLogging(builder => builder
            .AddDatabase<AppDbContext>(options =>
            {
                options.IncludeScopes = true;
                options.SyncInterval = 2;
            })
        )
        .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
```

##### Configuration example (appsettings.json)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    },
    "Database": {
      "IncludeScopes": false,
      "SyncInterval": 2,
      "LogLevel": {
        "Microsoft": "Information"
      }
    }
  }
}
```

##### Database context configuration
```CSharp
public class AppDbContext : DbContext, IDatabaseLoggerDbContext
{
    public DbSet<DatabaseLoggerLogEntry> SysLogEntries { get; set; }

    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<DatabaseLoggerLogEntry> GetLoggerDbSet() => SysLogEntries;
}
```