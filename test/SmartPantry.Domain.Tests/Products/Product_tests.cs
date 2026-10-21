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
        product.CodigoBarras.ShouldBe("123456789");
        product.Nombre.ShouldBe("Fideos");
        product.Marca.ShouldBe("Lucchetti");
        product.Ingredientes.ShouldBe("Trigo");
        product.Alergenos.ShouldBe("Gluten");
    }
}