using PruebaTecnicaInnova.Models;

namespace PruebaTecnicaInnova.Repositories
{
    public class InMemoryProductRepository : IProductRepository
    {
        private readonly object _sync = new();
        private readonly List<Product> _products = [];
        private int _nextId = 1;



        public InMemoryProductRepository()
        {
            if (_products.Count == 0)
            {
                _products.Add(new Product { Id = _nextId++, Name = "PC Gamer", Description = "PC gaming 1TB", Price = 1500.00m });
                _products.Add(new Product { Id = _nextId++, Name = "Iphone 16", Description = "Teléfono con cámara de alta resolución", Price = 800.00m });
                _products.Add(new Product { Id = _nextId++, Name = "Auriculares Inalámbricos", Description = "Auriculares con cancelación de ruido", Price = 200.00m });
                _products.Add(new Product { Id = _nextId++, Name = "Teclado", Description = "Teclado retroiluminado", Price = 100.00m });
                _products.Add(new Product { Id = _nextId++, Name = "Monitor 4K", Description = "Monitor de alta resolución 4K", Price = 600.00m });
                _products.Add(new Product { Id = _nextId++, Name = "SMART TV 68 Pulgadas", Description = "SMART TV de alta resolución 4K", Price = 50.00m });
            }
        }

        public int Get_nextId()
        {
            return _nextId;
        }


        public Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            lock (_sync)
            {
                var products = _products.ToList();

                return Task.FromResult<IEnumerable<Product>>(
                    products
                );
            }
        }

        public Task<Product?> GetProductByIdAsync(int id)
        {
            lock (_sync)
            {
                var product = _products
                    .FirstOrDefault(p => p.Id == id);

                return Task.FromResult(product);
            }
        }

        public Task<Product> AddProductAsync(Product product)
        {
            ArgumentNullException.ThrowIfNull(product);

            lock (_sync)
            {
                product.Id = _nextId++;

                _products.Add(product);

                return Task.FromResult(product);
            }
        }
        public Task<Product?> UpdateProductAsync(Product product)
        {
            ArgumentNullException.ThrowIfNull(product);

            lock (_sync)
            {
                var existingProduct = _products
                    .FirstOrDefault(p => p.Id == product.Id);

                if (existingProduct is null)
                {
                    return Task.FromResult<Product?>(null);
                }

                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;

                return Task.FromResult<Product?>(existingProduct);
            }
        }
        public Task<bool> DeleteProductAsync(int id)
        {
            lock (_sync)
            {
                var product = _products
                    .FirstOrDefault(p => p.Id == id);

                if (product is null)
                {
                    return Task.FromResult(false);
                }

                _products.Remove(product);

                return Task.FromResult(true);
            }
        }
        // Método para obtener productos por precio mínimo Simulacion de Consulta
        public Task<IEnumerable<Product>> GetProductsByMinPriceAsync(
            decimal minPrice)
        {
            lock (_sync)
            {
                var products = _products
                    .Where(p => p.Price >= minPrice)
                    .ToList();

                return Task.FromResult<IEnumerable<Product>>(
                    products
                );
            }
        }
    }
}
