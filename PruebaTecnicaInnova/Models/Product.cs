using System.ComponentModel.DataAnnotations;

namespace PruebaTecnicaInnova.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre del producto debe tener entre 3 y 100 caracteres.")]
        public string? Name { get; set; }
        public string? Description { get; set; }
        [Required(ErrorMessage = "El precio del producto es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio del producto debe ser mayor que cero.")]
        public decimal Price { get; set; }
    }
}
