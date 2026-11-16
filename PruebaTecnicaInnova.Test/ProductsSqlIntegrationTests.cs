
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PruebaTecnicaInnova.Data;
using PruebaTecnicaInnova.Models;
using PruebaTecnicaInnova.Repositories;
using Xunit;

namespace PruebaTecnicaInnova.Test;

public class ProductsSqlIntegrationTests
{
    [Fact]
    public async Task PostProduct_ShouldPersistInSqlServer()
    {
        // Crear un nombre exclusivo para esta prueba
        var databaseName =
            $"PruebaTecnicaInnova_Test_{Guid.NewGuid():N}";

        var connectionString =
            $@"Server=(localdb)\MSSQLLocalDB;" +
            $"Database={databaseName};" +
            "Trusted_Connection=True;Pooling=False;";

        WebApplicationFactory<Program>? factory = null;

        try
        {
            // ARRANGE: configurar una base SQL exclusiva
            factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.RemoveAll<
                            DbContextOptions<ProductsDbContext>>();

                        services.AddDbContext<ProductsDbContext>(
                            options =>
                                options.UseSqlServer(connectionString));

                        services.RemoveAll<IProductRepository>();

                        services.AddScoped<
                            IProductRepository,
                            EfProductRepository>();
                    });
                });

            // Aplicar las migraciones a la base temporal
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetRequiredService<ProductsDbContext>();

                await db.Database.MigrateAsync();
            }

            using var client = factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    BaseAddress = new Uri("https://localhost")
                });

            var product = new Product
            {
                Name = "Producto SQL Test",
                Description = "Prueba de persistencia real",
                Price = 1200m
            };

            // ACT 1: crear el producto
            var postResponse = await client.PostAsJsonAsync(
                "/api/Products", product);

            Assert.Equal(
                HttpStatusCode.Created,
                postResponse.StatusCode);

            var created = await postResponse.Content
                .ReadFromJsonAsync<Product>();

            Assert.NotNull(created);
            Assert.True(created.Id > 0);

            // ACT 2: recuperar el producto
            var getResponse = await client.GetAsync(
                $"/api/Products/{created.Id}");

            Assert.Equal(
                HttpStatusCode.OK,
                getResponse.StatusCode);

            var saved = await getResponse.Content
                .ReadFromJsonAsync<Product>();

            // ASSERT: verificar la información persistida
            Assert.NotNull(saved);
            Assert.Equal(created.Id, saved.Id);
            Assert.Equal(product.Name, saved.Name);
            Assert.Equal(product.Price, saved.Price);
        }
        finally
        {
            // Liberar el servidor de pruebas
            factory?.Dispose();

            // Eliminar únicamente la base temporal
            var options = new DbContextOptionsBuilder<ProductsDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            await using var cleanup =
                new ProductsDbContext(options);

            await cleanup.Database.EnsureDeletedAsync();
        }
    }


    [Fact]
    public async Task PutAndDelete_ShouldPersistChangesInSqlServer()
    {
        // Base temporal exclusiva para esta prueba
        var databaseName =
            $"PruebaTecnicaInnova_Test_{Guid.NewGuid():N}";

        var connectionString =
            $@"Server=(localdb)\MSSQLLocalDB;" +
            $"Database={databaseName};" +
            "Trusted_Connection=True;Pooling=False;";

        WebApplicationFactory<Program>? factory = null;

        try
        {
            // ARRANGE: configurar SQL Server de pruebas
            factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.RemoveAll<
                            DbContextOptions<ProductsDbContext>>();

                        services.AddDbContext<ProductsDbContext>(
                            options => options.UseSqlServer(
                                connectionString));

                        services.RemoveAll<IProductRepository>();

                        services.AddScoped<
                            IProductRepository,
                            EfProductRepository>();
                    });
                });

            // Crear el esquema de la base temporal
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetRequiredService<ProductsDbContext>();

                await db.Database.MigrateAsync();
            }

            using var client = factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    BaseAddress = new Uri("https://localhost")
                });

            // 1. CREAR PRODUCTO
            var product = new Product
            {
                Name = "Producto original",
                Description = "Primera versión",
                Price = 100m
            };

            var postResponse = await client.PostAsJsonAsync(
                "/api/Products", product);

            Assert.Equal(
                HttpStatusCode.Created,
                postResponse.StatusCode);

            var created = await postResponse.Content
                .ReadFromJsonAsync<Product>();

            Assert.NotNull(created);

            // 2. ACTUALIZAR PRODUCTO
            var updatedProduct = new Product
            {
                Id = created.Id,
                Name = "Producto actualizado",
                Description = "Segunda versión",
                Price = 250m
            };

            var putResponse = await client.PutAsJsonAsync(
                $"/api/Products/{created.Id}",
                updatedProduct);

            Assert.Equal(
                HttpStatusCode.NoContent,
                putResponse.StatusCode);

            // 3. COMPROBAR LOS DATOS ACTUALIZADOS
            var saved = await client.GetFromJsonAsync<Product>(
                $"/api/Products/{created.Id}");

            Assert.NotNull(saved);
            Assert.Equal("Producto actualizado", saved.Name);
            Assert.Equal("Segunda versión", saved.Description);
            Assert.Equal(250m, saved.Price);

            // 4. ELIMINAR PRODUCTO
            var deleteResponse = await client.DeleteAsync(
                $"/api/Products/{created.Id}");

            Assert.Equal(
                HttpStatusCode.NoContent,
                deleteResponse.StatusCode);

            // 5. COMPROBAR QUE YA NO EXISTE
            var getResponse = await client.GetAsync(
                $"/api/Products/{created.Id}");

            Assert.Equal(
                HttpStatusCode.NotFound,
                getResponse.StatusCode);
        }
        finally
        {
            factory?.Dispose();

            // Solo se elimina la base temporal de esta prueba
            var options = new DbContextOptionsBuilder<
                ProductsDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            await using var cleanup =
                new ProductsDbContext(options);

            await cleanup.Database.EnsureDeletedAsync();
        }
    }

}
