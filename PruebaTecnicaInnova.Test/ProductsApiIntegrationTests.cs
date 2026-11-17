using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PruebaTecnicaInnova.Dtos;
using PruebaTecnicaInnova.Models;
using PruebaTecnicaInnova.Repositories;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PruebaTecnicaInnova.Test;

public class ProductsApiIntegrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductsApiIntegrationTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IProductRepository>();

                services.AddSingleton<
                    IProductRepository,
                    InMemoryProductRepository>();
            });
        })
        .CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Theory]
    [InlineData("AB", 100)]
    [InlineData("Producto válido", -100)]
    public async Task PostProduct_InvalidData_Returns400(
        string name,
        int price)
    {
        // Arrange
        var product = new
        {
            name,
            description = "Prueba de integración",
            price
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/Products",
            product);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task GetProduct_NotFound_Returns404()
    {
        // Act
        var response = await _client.GetAsync(
            "/api/Products/999999");

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task PostProduct_ValidData_Returns201_AndCanBeRetrieved()
    {
        // ARRANGE: preparar un producto válido
        var newProduct = new Product
        {
            Name = $"Producto-{Guid.NewGuid():N}",
            Description = "Prueba de integración HTTP",
            Price = 150.00m
        };

        // ACT 1: crear producto mediante HTTP
        var createResponse = await _client.PostAsJsonAsync(
            "/api/Products",
            newProduct
        );

        // ASSERT 1: comprobar creación exitosa
        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode
        );

        var createdProduct = await createResponse.Content
            .ReadFromJsonAsync<Product>();

        Assert.NotNull(createdProduct);
        Assert.True(createdProduct.Id > 0);
        Assert.Equal(newProduct.Name, createdProduct.Name);

        // ACT 2: consultar el recurso recién creado
        Assert.NotNull(createResponse.Headers.Location);

        var getResponse = await _client.GetAsync(
            createResponse.Headers.Location
        );

        // ASSERT 2: verificar que el recurso existe
        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode
        );

        var savedProduct = await getResponse.Content
            .ReadFromJsonAsync<Product>();

        Assert.NotNull(savedProduct);
        Assert.Equal(createdProduct.Id, savedProduct.Id);
        Assert.Equal(newProduct.Name, savedProduct.Name);
        Assert.Equal(newProduct.Price, savedProduct.Price);
    }

    [Fact]
    public async Task PutProduct_MismatchedIds_Returns400()
    {
        // ARRANGE
        var request = new UpdateProductRequest
        {
            Id = 2,
            Name = "Producto actualizado",
            Description = "Prueba de IDs diferentes",
            Price = 150m
        };

        // ACT: el ID de la URL es diferente al del cuerpo
        var response = await _client.PutAsJsonAsync(
            "/api/Products/1",
            request);

        // ASSERT
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task GetProduct_ReturnsProductResponseContract()
    {
        // ARRANGE: crear un producto mediante HTTP
        var request = new CreateProductRequest
        {
            Name = $"Contrato-{Guid.NewGuid():N}",
            Description = "Prueba del contrato HTTP",
            Price = 250m
        };

        var postResponse = await _client.PostAsJsonAsync(
            "/api/Products",
            request);

        Assert.Equal(
            HttpStatusCode.Created,
            postResponse.StatusCode);

        var created = await postResponse.Content
            .ReadFromJsonAsync<ProductResponse>();

        Assert.NotNull(created);
        Assert.True(created.Id > 0);

        // ACT: consultar el producto creado
        var getResponse = await _client.GetAsync(
            $"/api/Products/{created.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        // ASSERT: verificar el contrato JSON
        var responseBody =
            await getResponse.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(responseBody);

        var root = document.RootElement;

        Assert.Equal(
            JsonValueKind.Object,
            root.ValueKind);

        // Solo los cuatro campos definidos por el DTO
        var properties = root
            .EnumerateObject()
            .Select(p => p.Name)
            .OrderBy(name => name)
            .ToArray();

        Assert.Equal(
            new[] { "description", "id", "name", "price" },
            properties);

        Assert.Equal(
            created.Id,
            root.GetProperty("id").GetInt32());

        Assert.Equal(
            request.Name,
            root.GetProperty("name").GetString());

        Assert.Equal(
            request.Description,
            root.GetProperty("description").GetString());

        Assert.Equal(
            request.Price,
            root.GetProperty("price").GetDecimal());
    }

    [Theory]
    [InlineData("/api/Products")]
    [InlineData("/api/Products/minprice/100")]
    public async Task GetProducts_ReturnsProductResponseCollection(
        string endpoint)
    {
        // ARRANGE: crear un producto reconocible
        var request = new CreateProductRequest
        {
            Name = $"Producto-{Guid.NewGuid():N}",
            Description = "Prueba de contrato de colección",
            Price = 250m
        };

        var postResponse = await _client.PostAsJsonAsync(
            "/api/Products",
            request);

        Assert.Equal(
            HttpStatusCode.Created,
            postResponse.StatusCode);

        var created = await postResponse.Content
            .ReadFromJsonAsync<ProductResponse>();

        Assert.NotNull(created);

        // ACT: consultar la colección
        var response = await _client.GetAsync(endpoint);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(json);

        var root = document.RootElement;

        Assert.Equal(JsonValueKind.Array, root.ValueKind);
        Assert.True(root.GetArrayLength() > 0);

        // ASSERT: validar el contrato de cada producto
        foreach (var product in root.EnumerateArray())
        {
            var properties = product
                .EnumerateObject()
                .Select(p => p.Name)
                .OrderBy(name => name)
                .ToArray();

            Assert.Equal(
                new[] { "description", "id", "name", "price" },
                properties);
        }

        // Confirmar que el producto creado está en la colección
        Assert.Contains(
            root.EnumerateArray(),
            product =>
                product.GetProperty("id").GetInt32() == created.Id);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    [InlineData(1000001, 20)]
    public async Task GetProductsPaged_InvalidParameters_Returns400(
        int pageNumber,
        int pageSize)
    {
        // ACT
        var response = await _client.GetAsync(
            $"/api/Products/paged?pageNumber={pageNumber}" +
            $"&pageSize={pageSize}");

        // ASSERT
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
}