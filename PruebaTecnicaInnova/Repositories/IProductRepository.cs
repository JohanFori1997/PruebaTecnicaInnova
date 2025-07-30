using PruebaTecnicaInnova.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PruebaTecnicaInnova.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product> AddProductAsync(Product product);
        Task<Product> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int id);
        Task<IEnumerable<Product>> GetProductsByMinPriceAsync(decimal minPrice);
    }
}
