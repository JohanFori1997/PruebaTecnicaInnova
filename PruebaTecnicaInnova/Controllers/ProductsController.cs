using Microsoft.AspNetCore.Mvc;
using PruebaTecnicaInnova.Models;
using PruebaTecnicaInnova.Repositories;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PruebaTecnicaInnova.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController(IProductRepository productRepository) : ControllerBase
    {
        private readonly IProductRepository _productRepository = productRepository;

        // GET: api/<ProductsController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _productRepository.GetAllProductsAsync();
            return Ok(products);
        }

        // GET api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult <Product>> GetProduct(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);

            if ( product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // GET api/Products/minprice/100
        [HttpGet("minprice/{minPrice}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsByMinPrice(decimal minPrice)
        {
            var products = await _productRepository.GetProductsByMinPriceAsync(minPrice);
            if (products == null || !products.Any())
            {
                return NotFound($"No se encontraron productos con un precio mínimo de {minPrice}.");
            }
            return Ok(products);
        }

        // POST api/Products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdProduct = await _productRepository.AddProductAsync(product);
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
        }

        // PUT api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest("El ID del producto no coincide con el ID en la URL.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedProduct = await _productRepository.UpdateProductAsync(product);
            if (updatedProduct == null)
            {
                return NotFound($"No se encontró el producto con ID {id}.");
            }

            return NoContent();
        }

        // DELETE api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducts(int id)
        {
            var success = await _productRepository.DeleteProductAsync(id);
            if (!success)
            {
                return NotFound($"No se encontró el producto con ID {id}.");
            }

            return NoContent();
        }
    }
}
