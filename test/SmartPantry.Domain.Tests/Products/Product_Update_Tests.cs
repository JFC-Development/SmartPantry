using System;
using Shouldly;
using SmartPantry.Domain.Products;
using Xunit;

namespace SmartPantry.Products;

public class Product_Update_Tests
{
    [Fact]
    public void Update_Con_Datos_Validos_Normaliza()
    {
        var product = new Product(Guid.NewGuid(), "7790001", "Leche", "Marca A");

        product.Update("  7790 002  ", "  Yerba  ", "  Marca B  ", "  Hojas  ", "  Ninguno  ");

        product.CodigoBarras.ShouldBe("7790002");
        product.Nombre.ShouldBe("Yerba");
        product.Marca.ShouldBe("Marca B");
        product.Ingredientes.ShouldBe("Hojas");
        product.Alergenos.ShouldBe("Ninguno");
    }

    [Fact]
    public void Update_Invalido_No_Deja_Cambios_Parciales()
    {
        var product = new Product(Guid.NewGuid(), "7790001", "Leche", "Marca A");
        var marcaLarga = new string('x', ProductConsts.MaxMarcaLength + 1);

        Should.Throw<ArgumentException>(() =>
            product.Update("7790002", "Nombre Nuevo", marcaLarga));

        // Nada cambió, ni siquiera lo que era válido
        product.CodigoBarras.ShouldBe("7790001");
        product.Nombre.ShouldBe("Leche");
        product.Marca.ShouldBe("Marca A");
    }
}