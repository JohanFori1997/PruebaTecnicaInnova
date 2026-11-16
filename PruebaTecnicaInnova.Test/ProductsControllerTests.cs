
using Microsoft.AspNetCore.Mvc;
using PruebaTecnicaInnova.Controllers;
using PruebaTecnicaInnova.Models;
using PruebaTecnicaInnova.Repositories;
using Xunit;

namespace PruebaTecnicaInnova.Test;

public class ProductsControllerTests
{
    [Fact]
    public async Task PutProduct_WhenIdDoesNotExist_Returns404()
    {
        // Arrange: preparar un producto inexistente
        var repository = new InMemoryProductRepository();
        var controller = new ProductsController(repository);

        var product = new Product
        {
            Id = 999999,
            Name = "Producto de prueba",
            Description = "Prueba de actualización",
            Price = 100m
        };

        // Act: intentar actualizar el producto
        var result = await controller.PutProduct(
            product.Id, product);

        // Assert: comprobar la respuesta esperada
        var notFound = Assert.IsType<NotFoundObjectResult>(
            result);

        Assert.Equal(404, notFound.StatusCode);
    }


    [Fact]
    public async Task PutProduct_WhenProductExists_Returns204()
    {
        // ARRANGE: preparar el repositorio y controlador
        var repository = new InMemoryProductRepository();
        var controller = new ProductsController(repository);

        // El producto con ID 1 existe en los datos iniciales
        var updatedProduct = new Product
        {
            Id = 1,
            Name = "PC Gamer Actualizado",
            Description = "Computador de alto rendimiento",
            Price = 2500.00m
        };

        // ACT: actualizar el producto
        var result = await controller.PutProduct(
            updatedProduct.Id,
            updatedProduct
        );

        // ASSERT 1: verificar la respuesta HTTP esperada
        var noContent = Assert.IsType<NoContentResult>(result);

        Assert.Equal(204, noContent.StatusCode);

        // ASSERT 2: consultar nuevamente el producto
        var savedProduct = await repository
            .GetProductByIdAsync(updatedProduct.Id);

        Assert.NotNull(savedProduct);

        // ASSERT 3: verificar que los cambios se guardaron
        Assert.Equal(updatedProduct.Name, savedProduct.Name);
        Assert.Equal(updatedProduct.Description, savedProduct.Description);
        Assert.Equal(updatedProduct.Price, savedProduct.Price);
    }

}
