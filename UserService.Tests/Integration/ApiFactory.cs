using Microsoft.Extensions.Configuration;
using UserService.Services;

namespace UserService.Tests.Integration;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UserService.Data;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Satisfies Program.cs validation.
                // The production provider will be replaced below.
                ["DatabaseSettings:ConnectionString"] =
                    "Host=test;Database=ignored"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<UserDbContext>();
            services.RemoveAll<DbContextOptions<UserDbContext>>();

            services.AddDbContext<UserDbContext>(options =>
                options.UseInMemoryDatabase(databaseName));
        });
    }
}