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
    }
}