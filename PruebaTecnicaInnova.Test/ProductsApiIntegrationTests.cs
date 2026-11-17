using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PruebaTecnicaInnova.Dtos;
using PruebaTecnicaInnova.Models;
using PruebaTecnicaInnova.Repositories;
using System.Net;
using System.Net.Http.Json;

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
}
