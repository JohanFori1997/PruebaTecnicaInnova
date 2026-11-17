using Microsoft.AspNetCore.Mvc;
using PruebaTecnicaInnova.Common;
using PruebaTecnicaInnova.Dtos;
using PruebaTecnicaInnova.Models;
using PruebaTecnicaInnova.Repositories;
using System.ComponentModel.DataAnnotations;

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
        public async Task<ActionResult<IEnumerable<ProductResponse>>>
            GetProducts()
        {
            // Obtener entidades desde el repositorio
            var products =
                await _productRepository.GetAllProductsAsync();

            // Convertir entidades a DTOs
            var response = products
                .Select(ToResponse)
                .ToList();

            return Ok(response);
        }

        // GET api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductResponse>> GetProduct(int id)
        {
            // Consultar la entidad mediante el repositorio
            var product = await _productRepository
                .GetProductByIdAsync(id);

            if (product is null)
            {
                return NotFound();
            }

            // Transformar la entidad en un DTO público
            var response = new ProductResponse(
                product.Id,
                product.Name,
                product.Description,
                product.Price
            );

            return Ok(response);
        }

        // GET api/Products/minprice/100
        [HttpGet("minprice/{minPrice}")]
        public async Task<ActionResult<IEnumerable<ProductResponse>>>
            GetProductsByMinPrice(decimal minPrice)
        {
            var products = await _productRepository
                .GetProductsByMinPriceAsync(minPrice);

            var response = products
                .Select(ToResponse)
                .ToList();

            if (response.Count == 0)
            {
                return NotFound(
                    $"No se encontraron productos con un precio mínimo de {minPrice}.");
            }

            return Ok(response);
        }

        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<ProductResponse>>>
            GetProductsPaged(
                [FromQuery, Range(1, 1000000)] int pageNumber = 1,
                [FromQuery, Range(1, 100)] int pageSize = 20)
        {
            var result = await _productRepository
                .GetPagedProductsAsync(pageNumber, pageSize);

            var response = new PagedResult<ProductResponse>(
                result.Items.Select(ToResponse).ToList(),
                result.PageNumber,
                result.PageSize,
                result.TotalCount);

            return Ok(response);
        }

        // POST api/Products
        [HttpPost]
        public async Task<ActionResult<ProductResponse>> PostProduct(
            [FromBody] CreateProductRequest request)
        {
            // Convertir el DTO de entrada en una entidad
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price
            };

            // Persistir mediante el repositorio
            var createdProduct =
                await _productRepository.AddProductAsync(product);

            // Preparar la respuesta HTTP
            var response = new ProductResponse(
                createdProduct.Id,
                createdProduct.Name,
                createdProduct.Description,
                createdProduct.Price
            );

            return CreatedAtAction(
                nameof(GetProduct),
                new { id = createdProduct.Id },
                response
            );
        }


        // PUT api/Products/5

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(
            int id,
            [FromBody] UpdateProductRequest request)
        {
            // Validar que el ID de la ruta y del cuerpo coincidan
            if (id != request.Id)
            {
                return BadRequest(
                    "El ID del producto no coincide con el ID en la URL.");
            }

            // Convertir el DTO en la entidad correspondiente
            var product = new Product
            {
                Id = request.Id,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price
            };

            // Ejecutar la actualización mediante el repositorio
            var updatedProduct =
                await _productRepository.UpdateProductAsync(product);

            if (updatedProduct is null)
            {
                return NotFound(
                    $"No se encontró el producto con ID {id}.");
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
        private static ProductResponse ToResponse(Product product)
        {
            return new ProductResponse(
                product.Id,
                product.Name,
                product.Description,
                product.Price
            );
        }

    }
}
