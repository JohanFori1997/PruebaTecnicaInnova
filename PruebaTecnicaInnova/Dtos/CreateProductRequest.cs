
using System.ComponentModel.DataAnnotations;

namespace PruebaTecnicaInnova.Dtos;

public sealed class CreateProductRequest
{
    [Required(
        ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(
        100,
        MinimumLength = 3,
        ErrorMessage =
            "El nombre debe tener entre 3 y 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }


    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true,
        ErrorMessage =
            "El precio debe ser mayor que cero y estar dentro del rango permitido.")]
    public decimal Price { get; set; }
}
