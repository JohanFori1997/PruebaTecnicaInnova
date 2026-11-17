
using PruebaTecnicaInnova.Common;
using PruebaTecnicaInnova.Dtos;
using PruebaTecnicaInnova.Models;
using PruebaTecnicaInnova.Test.Infrastructure;
using System.Net;
using System.Net.Http.Json;

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

    [Fact]
    public async Task GetProductsPaged_ShouldReturnCorrectSqlPages()
    {
        await using var environment =
            await SqlServerTestEnvironment.CreateAsync();

        var client = environment.Client;
        var createdIds = new List<int>();

        // ARRANGE: insertar cinco productos en SQL Server
        for (int i = 1; i <= 5; i++)
        {
            var request = new CreateProductRequest
            {
                Name = $"Producto paginado {i}",
                Description = "Prueba SQL de paginación",
                Price = 100m * i
            };

            var postResponse = await client.PostAsJsonAsync(
                "/api/Products", request);

            Assert.Equal(
                HttpStatusCode.Created,
                postResponse.StatusCode);

            var created = await postResponse.Content
                .ReadFromJsonAsync<ProductResponse>();

            Assert.NotNull(created);
            createdIds.Add(created.Id);
        }

        // ACT: solicitar primera y tercera página
        var firstPage = await client
            .GetFromJsonAsync<PagedResult<ProductResponse>>(
                "/api/Products/paged?pageNumber=1&pageSize=2");

        var thirdPage = await client
            .GetFromJsonAsync<PagedResult<ProductResponse>>(
                "/api/Products/paged?pageNumber=3&pageSize=2");

        // ASSERT: validar metadatos
        Assert.NotNull(firstPage);
        Assert.NotNull(thirdPage);

        Assert.Equal(5, firstPage.TotalCount);
        Assert.Equal(3, firstPage.TotalPages);
        Assert.Equal(1, firstPage.PageNumber);
        Assert.Equal(2, firstPage.PageSize);

        // La primera página contiene dos registros
        Assert.Equal(2, firstPage.Items.Count);

        // La tercera página contiene uno
        Assert.Single(thirdPage.Items);

        // Verificar que las páginas respetan el orden por ID
        Assert.Equal(
            createdIds.Take(2),
            firstPage.Items.Select(p => p.Id));

        Assert.Equal(
            createdIds.Skip(4),
            thirdPage.Items.Select(p => p.Id));
    }
    [Fact]
    public async Task GetProductsPaged_PageBeyondLast_ReturnsEmptyItems()
    {
        // ARRANGE
        await using var environment =
            await SqlServerTestEnvironment.CreateAsync();

        var client = environment.Client;

        var request = new CreateProductRequest
        {
            Name = "Producto único",
            Description = "Prueba de página vacía",
            Price = 100m
        };

        var postResponse = await client.PostAsJsonAsync(
            "/api/Products", request);

        Assert.Equal(
            HttpStatusCode.Created,
            postResponse.StatusCode);

        // ACT: solicitar la segunda página
        var response = await client.GetAsync(
            "/api/Products/paged?pageNumber=2&pageSize=1");

        // ASSERT
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductResponse>>();

        Assert.NotNull(result);

        Assert.Empty(result.Items);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(1, result.PageSize);
    }
}
