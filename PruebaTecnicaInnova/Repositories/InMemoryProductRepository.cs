using PruebaTecnicaInnova.Models;

namespace PruebaTecnicaInnova.Repositories
{
    public class InMemoryProductRepository : IProductRepository
    {
        private static readonly List<Product> products = [];
        private readonly List<Product> _products = products;
        private int _nextId = 1;

        public InMemoryProductRepository()
        {
            if (products.Count == 0)
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

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await Task.FromResult(_products.AsEnumerable());
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            return await Task.FromResult(product);
        }

        public async Task<Product> AddProductAsync(Product product)
        {
            ArgumentNullException.ThrowIfNull(product);

            product.Id = _nextId++;
            _products.Add(product);
            return await Task.FromResult(product);
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            ArgumentNullException.ThrowIfNull(product);

            var existingProduct = _products.FirstOrDefault(p => p.Id == product.Id) ?? throw new KeyNotFoundException($"No se encontró el producto con Id {product.Id}");
            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;

            return await Task.FromResult(existingProduct);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var productRemove = _products.FirstOrDefault(p => p.Id == id);
            if (productRemove == null) return await Task.FromResult(false);

            _products.Remove(productRemove);
            return await Task.FromResult(true);
        }

        // Método para obtener productos por precio mínimo Simulacion de Consulta
        public async Task<IEnumerable<Product>> GetProductsByMinPriceAsync(decimal minPrice)
        {
            return await Task.FromResult(_products.Where(p => p.Price >= minPrice).ToList());
        }
    }
}
