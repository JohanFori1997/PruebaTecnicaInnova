using System.ComponentModel.DataAnnotations;
using System.Globalization;
using PruebaTecnicaInnova.Dtos;

namespace PruebaTecnicaInnova.Test;

public class CreateProductRequestTests
{
    [Theory]
    [InlineData("es-CO")]
    [InlineData("en-US")]
    public void PriceValidation_ShouldWorkWithDifferentCultures(
        string cultureName)
    {
        var originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture =
                CultureInfo.GetCultureInfo(cultureName);

            // Precio válido
            var validRequest = new CreateProductRequest
            {
                Name = "Producto válido",
                Price = 125.50m
            };

            Assert.True(IsValid(validRequest));

            // Precio inválido
            var invalidRequest = new CreateProductRequest
            {
                Name = "Producto inválido",
                Price = -100m
            };

            Assert.False(IsValid(invalidRequest));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    private static bool IsValid(CreateProductRequest request)
    {
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        return Validator.TryValidateObject(
            request,
            context,
            results,
            validateAllProperties: true);
    }
}
