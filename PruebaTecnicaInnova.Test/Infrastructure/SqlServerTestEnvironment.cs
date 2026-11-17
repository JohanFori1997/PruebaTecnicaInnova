
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PruebaTecnicaInnova.Data;
using PruebaTecnicaInnova.Repositories;

namespace PruebaTecnicaInnova.Test.Infrastructure;

public sealed class SqlServerTestEnvironment : IAsyncDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _connectionString;

    public HttpClient Client { get; }

    private SqlServerTestEnvironment(
        WebApplicationFactory<Program> factory,
        HttpClient client,
        string connectionString)
    {
        _factory = factory;
        Client = client;
        _connectionString = connectionString;
    }

    public static async Task<SqlServerTestEnvironment> CreateAsync()
    {
        var databaseName =
            $"PruebaTecnicaInnova_Test_{Guid.NewGuid():N}";

        var connectionString =
            $@"Server=(localdb)\MSSQLLocalDB;" +
            $"Database={databaseName};" +
            "Trusted_Connection=True;Pooling=False;";

        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<
                        DbContextOptions<ProductsDbContext>>();

                    services.RemoveAll<IProductRepository>();

                    services.AddDbContext<ProductsDbContext>(
                        options => options.UseSqlServer(
                            connectionString));

                    services.AddScoped<
                        IProductRepository,
                        EfProductRepository>();
                });
            });

        try
        {
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetRequiredService<ProductsDbContext>();

                await db.Database.MigrateAsync();
            }

            var client = factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    BaseAddress = new Uri("https://localhost")
                });

            return new SqlServerTestEnvironment(
                factory, client, connectionString);
        }
        catch
        {
            factory.Dispose();

            // Intentar limpiar la base si falla la inicialización
            try
            {
                await DeleteTestDatabaseAsync(connectionString);
            }
            catch
            {
                // Conservar la excepción original
            }

            throw;
        }
    }

    private static async Task DeleteTestDatabaseAsync(
        string connectionString)
    {
        var options = new DbContextOptionsBuilder<ProductsDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var db = new ProductsDbContext(options);

        await db.Database.EnsureDeletedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        _factory.Dispose();

        await DeleteTestDatabaseAsync(_connectionString);
    }
}
