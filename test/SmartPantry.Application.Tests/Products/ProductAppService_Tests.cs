using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace SmartPantry.Products;

public abstract class ProductAppService_Tests<TStartupModule> : SmartPantryApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IProductAppService _productAppService;

    protected ProductAppService_Tests()
    {
        _productAppService = GetRequiredService<IProductAppService>();
    }

    [Fact]
    public async Task Should_Register_List_Update_Get_And_Delete_A_Product()
    {
        // Registrar
        var creado = await _productAppService.CreateAsync(
            new CreateProductDto
            {
                CodigoBarras = Guid.NewGuid().ToString("N")[..13],
                Nombre = "Producto de prueba CRUD",
                Marca = "Marca A"
            }
        );
        creado.Id.ShouldNotBe(Guid.Empty);

        // Listar (paginado y ordenado)
        var lista = await _productAppService.GetListAsync(
            new PagedAndSortedResultRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 100,
                Sorting = "Nombre"
            }
        );
        lista.TotalCount.ShouldBeGreaterThan(0);
        lista.Items.ShouldContain(p => p.Id == creado.Id);

        // Modificar (con espacios, para comprobar que se normaliza)
        var modificado = await _productAppService.UpdateAsync(
            creado.Id,
            new UpdateProductDto
            {
                CodigoBarras = creado.CodigoBarras,
                Nombre = "  Producto modificado  ",
                Marca = "Marca B"
            }
        );
        modificado.Nombre.ShouldBe("Producto modificado");

        // Consultar
        var consultado = await _productAppService.GetAsync(creado.Id);
        consultado.Nombre.ShouldBe("Producto modificado");
        consultado.Marca.ShouldBe("Marca B");

        // Eliminar y comprobar que ya no existe
        await _productAppService.DeleteAsync(creado.Id);
        await Should.ThrowAsync<EntityNotFoundException>(async () =>
        {
            await _productAppService.GetAsync(creado.Id);
        });
    }

    [Fact]
    public async Task Should_Not_Create_A_Product_Without_Name()
    {
        var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
        {
            await _productAppService.CreateAsync(
                new CreateProductDto
                {
                    CodigoBarras = "7790001",
                    Nombre = ""
                }
            );
        });

        exception.ValidationErrors
            .ShouldContain(err => err.MemberNames.Any(mem => mem == "Nombre"));
    }
}
