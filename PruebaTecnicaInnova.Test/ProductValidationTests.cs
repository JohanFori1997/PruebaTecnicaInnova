
using System.ComponentModel.DataAnnotations;
using PruebaTecnicaInnova.Models;
using Xunit;

namespace PruebaTecnicaInnova.Test;

public class ProductValidationTests
{
    [Theory]
    [InlineData("AB", false)]
    [InlineData("ABC", true)]
    public void Product_NameLength_IsValidated(
        string name,
        bool expectedValid)
    {
        // ARRANGE
        var product = new Product
        {
            Name = name,
            Description = "Producto de prueba",
            Price = 100m
        };

        var results = new List<ValidationResult>();

        var context = new ValidationContext(product);

        // ACT
        bool isValid = Validator.TryValidateObject(
            product,
            context,
            results,
            validateAllProperties: true
        );

        // ASSERT
        Assert.Equal(expectedValid, isValid);

        if (!expectedValid)
        {
            Assert.Contains(
                results,
                r => r.MemberNames.Contains(
                    nameof(Product.Name))
            );
        }
    }

    [Fact]
    public void Product_NegativePrice_IsInvalid()
    {
        // ARRANGE
        var product = new Product
        {
            Name = "Producto válido",
            Price = -100m
        };

        var results = new List<ValidationResult>();
        var context = new ValidationContext(product);

        // ACT
        bool isValid = Validator.TryValidateObject(
            product,
            context,
            results,
            validateAllProperties: true
        );

        // ASSERT
        Assert.False(isValid);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(
                nameof(Product.Price))
        );
    }
}
