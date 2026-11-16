using PruebaTecnicaInnova.Models;
using PruebaTecnicaInnova.Repositories;

namespace PruebaTecnicaInnova.Test
{
    public class ProductoRepositorioTest
    {
        [Fact]
        public async Task GetProductsByMinPriceAsync_CorrectProducts()
        {
            var repository = new InMemoryProductRepository();

            decimal minPrice = 100.00m;

            var result = await repository.GetProductsByMinPriceAsync(minPrice);
            Assert.NotNull(result);
            Assert.All(result, product => Assert.True(product.Price >= minPrice));
            Assert.Equal(5, result.Count());
        }

        [Fact]
        public async Task GetProductsByMinPriceAsync_NoProductsFound()
        {
            var repository = new InMemoryProductRepository();
            decimal minPrice = 10000.00m;
            var result = await repository.GetProductsByMinPriceAsync(minPrice);
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetProductsByMinPriceAsync_MinPriceIsZero()
        {
            var repository = new InMemoryProductRepository();
            decimal minPrice = 0.00m;
            var result = await repository.GetProductsByMinPriceAsync(minPrice);
            Assert.NotNull(result);
            Assert.Equal(6, result.Count());
        }


        [Fact]
        public async Task AddProductAsync_GeneratesUniqueIds()
        {
            // ARRANGE: crear repositorio independiente
            var repository = new InMemoryProductRepository();

            var initialProducts = (await repository
                .GetAllProductsAsync()).ToList();

            var initialIds = initialProducts
                .Select(p => p.Id)
                .ToHashSet();

            var product1 = new Product
            {
                Name = "Mouse Gamer",
                Description = "Mouse de prueba",
                Price = 150.00m
            };

            var product2 = new Product
            {
                Name = "Teclado Mecánico",
                Description = "Teclado de prueba",
                Price = 250.00m
            };

            // ACT: agregar dos productos
            var created1 = await repository.AddProductAsync(product1);
            var created2 = await repository.AddProductAsync(product2);

            // ASSERT: los nuevos IDs deben ser válidos y diferentes
            Assert.True(created1.Id > 0);
            Assert.True(created2.Id > 0);
            Assert.NotEqual(created1.Id, created2.Id);

            // Ningún ID nuevo debe existir previamente
            Assert.DoesNotContain(created1.Id, initialIds);
            Assert.DoesNotContain(created2.Id, initialIds);

            // Verificar la colección completa
            var allProducts = (await repository
                .GetAllProductsAsync()).ToList();

            Assert.Equal(initialProducts.Count + 2, allProducts.Count);

            Assert.Equal(
                allProducts.Count,
                allProducts.Select(p => p.Id).Distinct().Count()
            );
        }

    }
}