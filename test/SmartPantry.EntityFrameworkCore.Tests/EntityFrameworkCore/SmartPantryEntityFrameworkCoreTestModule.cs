using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;
using System;

namespace SmartPantry.EntityFrameworkCore;

[DependsOn(
    typeof(SmartPantryApplicationTestModule),
    typeof(SmartPantryEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule)
)]
public class SmartPantryEntityFrameworkCoreTestModule : AbpModule
{
    private static readonly string TestConnectionString =
    Environment.GetEnvironmentVariable("TEST_CONNECTION_STRING")
    ?? "Server=.\\SQLEXPRESS;Database=SmartPantry_Test;Trusted_Connection=True;TrustServerCertificate=true";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<FeatureManagementOptions>(options =>
        {
            options.SaveStaticFeaturesToDatabase = false;
            options.IsDynamicFeatureStoreEnabled = false;
        });
        Configure<PermissionManagementOptions>(options =>
        {
            options.SaveStaticPermissionsToDatabase = false;
            options.IsDynamicPermissionStoreEnabled = false;
        });
        context.Services.AddAlwaysDisableUnitOfWorkTransaction();

        ConfigureSqlServer(context.Services);
    }

    private void ConfigureSqlServer(IServiceCollection services)
    {
        var dbContextOptions = new DbContextOptionsBuilder<SmartPantryDbContext>()
            .UseSqlServer(TestConnectionString)
            .Options;

        using (var context = new SmartPantryDbContext(dbContextOptions))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        services.Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = TestConnectionString;
        });

        services.Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(context =>
            {
                context.UseSqlServer();
            });
        });
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        using var dbContext = new SmartPantryDbContext(
            new DbContextOptionsBuilder<SmartPantryDbContext>().UseSqlServer(TestConnectionString).Options);
        dbContext.Database.EnsureDeleted();
    }
}