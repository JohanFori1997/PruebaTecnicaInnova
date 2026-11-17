
using Microsoft.EntityFrameworkCore;
using PruebaTecnicaInnova.Common;
using PruebaTecnicaInnova.Data;
using PruebaTecnicaInnova.Models;

namespace PruebaTecnicaInnova.Repositories;

public sealed class EfProductRepository : IProductRepository
{
    private readonly ProductsDbContext _context;

    public EfProductRepository(ProductsDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync();
    }
    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    public async Task<Product> AddProductAsync(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        // SQL Server genera la clave primaria
        product.Id = 0;

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return product;
    }
    public async Task<Product?> UpdateProductAsync(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        var existingProduct = await _context.Products
            .FindAsync(product.Id);

        if (existingProduct is null)
        {
            return null;
        }

        existingProduct.Name = product.Name;
        existingProduct.Description = product.Description;
        existingProduct.Price = product.Price;

        await _context.SaveChangesAsync();

        return existingProduct;
    }
    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product is null)
        {
            return false;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return true;
    }
    public async Task<IEnumerable<Product>> GetProductsByMinPriceAsync(
        decimal minPrice)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.Price >= minPrice)
            .OrderBy(p => p.Id)
            .ToListAsync();
    }
    public async Task<PagedResult<Product>> GetPagedProductsAsync(
        int pageNumber,
        int pageSize)
    {
        var query = _context.Products.AsNoTracking();

        // Total de productos en SQL Server
        var totalCount = await query.CountAsync();

        // Calcular desplazamiento
        var offset = (pageNumber - 1) * pageSize;

        // Recuperar únicamente la página solicitada
        var products = await query
            .OrderBy(p => p.Id)
            .Skip(offset)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Product>(
            products,
            pageNumber,
            pageSize,
            totalCount);
    }
}
