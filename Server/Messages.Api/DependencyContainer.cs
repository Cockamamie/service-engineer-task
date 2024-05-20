using Messages.Api.Repositories;
using Ydb.Sdk;
using Ydb.Sdk.Yc;

namespace Messages.Api;

public class DependencyContainer
{
    public static void RegisterDependencies(IServiceCollection services)
    {
        services.AddSingleton<MessagesRepository>();
        services.AddSingleton<Driver>(GetDriver());
    }

    private static Driver GetDriver()
    {
        var loggerFactory = new LoggerFactory();
        var saFilePath = Environment.GetEnvironmentVariable("YDB_SERVICE_ACCOUNT_KEY_FILE") ??
                         throw new InvalidOperationException();
        var saProvider = new ServiceAccountProvider(
            saFilePath: saFilePath,
            loggerFactory: loggerFactory);
        saProvider.Initialize();

        var dbEndpoint = Environment.GetEnvironmentVariable("DB_ENDPOINT") ?? throw new InvalidOperationException();
        var db = Environment.GetEnvironmentVariable("DB") ?? throw new InvalidOperationException();
        var config = new DriverConfig(dbEndpoint, db, saProvider);
        var driver = new Driver(config, loggerFactory);
        driver.Initialize().ConfigureAwait(false).GetAwaiter().GetResult();

        return driver;
    }
}