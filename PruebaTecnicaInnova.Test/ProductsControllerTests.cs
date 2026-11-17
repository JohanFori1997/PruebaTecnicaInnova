
using Microsoft.AspNetCore.Mvc;
using PruebaTecnicaInnova.Controllers;
using PruebaTecnicaInnova.Dtos;
using PruebaTecnicaInnova.Repositories;
using Xunit;

namespace PruebaTecnicaInnova.Test;

public class ProductsControllerTests
{

    [Fact]
    public async Task PutProduct_WhenIdDoesNotExist_Returns404()
    {
        // ARRANGE
        var repository = new InMemoryProductRepository();
        var controller = new ProductsController(repository);

        const int nonexistentId = 999999;

        // Confirmar que el producto realmente no existe
        var existingProduct =
            await repository.GetProductByIdAsync(nonexistentId);

        Assert.Null(existingProduct);

        var request = new UpdateProductRequest
        {
            Id = nonexistentId,
            Name = "Producto inexistente",
            Description = "Prueba de actualización",
            Price = 150m
        };

        // ACT
        var result = await controller.PutProduct(
            nonexistentId,
            request);

        // ASSERT
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task PutProduct_WhenProductExists_Returns204()
    {
        // ARRANGE: preparar el repositorio y controlador
        var repository = new InMemoryProductRepository();
        var controller = new ProductsController(repository);

        // El producto con ID 1 existe en los datos iniciales
        var updatedProduct = new UpdateProductRequest
        {
            Id = 1,
            Name = "Producto actualizado",
            Description = "Descripción actualizada",
            Price = 200m
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
