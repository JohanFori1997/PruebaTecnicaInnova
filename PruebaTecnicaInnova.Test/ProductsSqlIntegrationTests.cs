
using System.Net;
using System.Net.Http.Json;
using PruebaTecnicaInnova.Models;
using PruebaTecnicaInnova.Test.Infrastructure;

namespace PruebaTecnicaInnova.Test;

public class ProductsSqlIntegrationTests
{

    [Fact]
    public async Task PostProduct_ShouldPersistInSqlServer()
    {
        // ARRANGE: crear un entorno SQL aislado
        await using var environment =
            await SqlServerTestEnvironment.CreateAsync();

        var client = environment.Client;

        var product = new Product
        {
            Name = "Producto SQL Test",
            Description = "Prueba de persistencia real",
            Price = 1200m
        };

        // ACT: crear producto
        var response = await client.PostAsJsonAsync(
            "/api/Products", product);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var created = await response.Content
            .ReadFromJsonAsync<Product>();

        Assert.NotNull(created);
        Assert.True(created.Id > 0);

        // ASSERT: consultar lo persistido
        var getResponse = await client.GetAsync(
            $"/api/Products/{created.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        var saved = await getResponse.Content
            .ReadFromJsonAsync<Product>();

        Assert.NotNull(saved);
        Assert.Equal(created.Id, saved.Id);
        Assert.Equal(product.Name, saved.Name);
        Assert.Equal(product.Price, saved.Price);
    }


    [Fact]
    public async Task PutAndDelete_ShouldPersistChangesInSqlServer()
    {
        // ARRANGE: crear una base SQL temporal
        await using var environment =
            await SqlServerTestEnvironment.CreateAsync();

        var client = environment.Client;

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
        Assert.True(created.Id > 0);

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

        // 3. VERIFICAR LA ACTUALIZACIÓN
        var getResponse = await client.GetAsync(
            $"/api/Products/{created.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        var saved = await getResponse.Content
            .ReadFromJsonAsync<Product>();

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

        // 5. COMPROBAR LA ELIMINACIÓN
        var finalResponse = await client.GetAsync(
            $"/api/Products/{created.Id}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            finalResponse.StatusCode);
    }
}
