using System;
using Shouldly;
using Xunit;
using SmartPantry.Domain.Products;

namespace SmartPantry.Products;

public class Product_Tests
{
    [Fact]
    public void Should_Create_Valid_Product()
    {
        var product = new Product(
            Guid.NewGuid(),
            "123456789",
            "Fideos",
            "Lucchetti",
            "Trigo",
            "Gluten"
        );

        product.Nombre.ShouldBe("Fideos");
    }
}