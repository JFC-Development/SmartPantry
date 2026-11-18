using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.Users;
using Volo.Abp.Validation;
using Xunit;
using SmartPantry.Domain.Products;

namespace SmartPantry.Despensa;

public abstract class PantryItemAppService_Tests<TStartupModule> : SmartPantryApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IPantryItemAppService _service;
    private readonly IRepository<ItemDespensa, Guid> _items;
    private readonly IRepository<Product, Guid> _products;
    private readonly ICurrentUser _currentUser;

    protected PantryItemAppService_Tests()
    {
        _service = GetRequiredService<IPantryItemAppService>();
        _items = GetRequiredService<IRepository<ItemDespensa, Guid>>();
        _products = GetRequiredService<IRepository<Product, Guid>>();
        _currentUser = GetRequiredService<ICurrentUser>();
    }

    private async Task<Guid> SembrarAsync(
        Guid propietarioId,
        string nombreProducto = "Producto de prueba",
        decimal? cantidad = 2,
        string? unidad = "kg",
        DateTime? vencimiento = null)
    {
        var productoId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            await _products.InsertAsync(new Product(
                productoId, Guid.NewGuid().ToString("N")[..13], nombreProducto));

            await _items.InsertAsync(new ItemDespensa(
                itemId, propietarioId, productoId,
                ItemDespensaConsts.EstadoEnStock, vencimiento, cantidad, unidad));
        });

        return itemId;
    }

    private Task<ItemDespensa?> BuscarEnBaseAsync(Guid id) =>
        WithUnitOfWorkAsync(() => _items.FindAsync(id));

    [Fact]
    public async Task Should_List_Only_Own_Items_With_Product_Name()
    {
        var propio = await SembrarAsync(_currentUser.Id!.Value, "Leche entera");
        var ajeno = await SembrarAsync(Guid.NewGuid(), "Producto ajeno");

        var lista = await _service.GetListAsync(
            new PagedAndSortedResultRequestDto { SkipCount = 0, MaxResultCount = 1000 });

        lista.Items.ShouldContain(i => i.Id == propio);
        lista.Items.ShouldNotContain(i => i.Id == ajeno);
        lista.Items.Single(i => i.Id == propio).NombreProducto.ShouldBe("Leche entera");
    }

    [Fact]
    public async Task Should_Return_Null_Optional_Fields_When_Data_Is_Missing()
    {
        var id = await SembrarAsync(_currentUser.Id!.Value, cantidad: null, unidad: null, vencimiento: null);

        var lista = await _service.GetListAsync(
            new PagedAndSortedResultRequestDto { SkipCount = 0, MaxResultCount = 1000 });

        var dto = lista.Items.Single(i => i.Id == id);
        dto.Cantidad.ShouldBeNull();
        dto.Unidad.ShouldBeNull();
        dto.FechaVencimiento.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Update_Own_Item()
    {
        var id = await SembrarAsync(_currentUser.Id!.Value);
        var nuevaFecha = new DateTime(2027, 1, 15);

        var dto = await _service.UpdateAsync(id, new UpdatePantryItemDto
        {
            Cantidad = 5,
            Unidad = "  litros  ",
            FechaVencimiento = nuevaFecha
        });

        dto.Cantidad.ShouldBe(5);
        dto.Unidad.ShouldBe("litros");          // se normaliza (Trim)
        dto.FechaVencimiento.ShouldBe(nuevaFecha);

        var enBase = await BuscarEnBaseAsync(id);
        enBase!.Cantidad.ShouldBe(5);
        enBase.FechaVencimiento.ShouldBe(nuevaFecha);
    }

    [Fact]
    public async Task Should_Not_Update_With_Negative_Quantity()
    {
        var id = await SembrarAsync(_currentUser.Id!.Value);

        var ex = await Assert.ThrowsAsync<AbpValidationException>(async () =>
        {
            await _service.UpdateAsync(id, new UpdatePantryItemDto { Cantidad = -1 });
        });

        ex.ValidationErrors.ShouldContain(e => e.MemberNames.Any(m => m == "Cantidad"));
    }

    [Fact]
    public async Task Should_Not_Update_With_Too_Long_Unit()
    {
        var id = await SembrarAsync(_currentUser.Id!.Value);

        var ex = await Assert.ThrowsAsync<AbpValidationException>(async () =>
        {
            await _service.UpdateAsync(id, new UpdatePantryItemDto
            {
                Unidad = new string('x', ItemDespensaConsts.MaxUnidadLength + 1)
            });
        });

        ex.ValidationErrors.ShouldContain(e => e.MemberNames.Any(m => m == "Unidad"));
    }

    [Fact]
    public async Task Should_Mark_Own_Item_As_Consumed()
    {
        var id = await SembrarAsync(_currentUser.Id!.Value);

        var dto = await _service.MarkAsConsumedAsync(id);

        dto.Estado.ShouldBe(ItemDespensaConsts.EstadoConsumido);
        (await BuscarEnBaseAsync(id))!.Estado.ShouldBe(ItemDespensaConsts.EstadoConsumido);
    }

    [Fact]
    public async Task Should_Delete_Own_Item()
    {
        var id = await SembrarAsync(_currentUser.Id!.Value);

        await _service.DeleteAsync(id);

        // Borrado lógico: ya no se ve ni en la base (filtro de ABP) ni en el listado.
        (await BuscarEnBaseAsync(id)).ShouldBeNull();
        var lista = await _service.GetListAsync(
            new PagedAndSortedResultRequestDto { SkipCount = 0, MaxResultCount = 1000 });
        lista.Items.ShouldNotContain(i => i.Id == id);
    }

    [Fact]
    public async Task Should_Not_Update_Another_Users_Item()
    {
        var ajeno = await SembrarAsync(Guid.NewGuid(), cantidad: 1, unidad: "u");

        await Should.ThrowAsync<EntityNotFoundException>(async () =>
        {
            await _service.UpdateAsync(ajeno, new UpdatePantryItemDto { Cantidad = 99, Unidad = "hack" });
        });

        var enBase = await BuscarEnBaseAsync(ajeno);
        enBase!.Cantidad.ShouldBe(1);
        enBase.Unidad.ShouldBe("u");
    }

    [Fact]
    public async Task Should_Not_Mark_Another_Users_Item_As_Consumed()
    {
        var ajeno = await SembrarAsync(Guid.NewGuid());

        await Should.ThrowAsync<EntityNotFoundException>(async () =>
        {
            await _service.MarkAsConsumedAsync(ajeno);
        });

        (await BuscarEnBaseAsync(ajeno))!.Estado.ShouldBe(ItemDespensaConsts.EstadoEnStock);
    }

    [Fact]
    public async Task Should_Not_Delete_Another_Users_Item()
    {
        var ajeno = await SembrarAsync(Guid.NewGuid());

        await Should.ThrowAsync<EntityNotFoundException>(async () =>
        {
            await _service.DeleteAsync(ajeno);
        });

        (await BuscarEnBaseAsync(ajeno)).ShouldNotBeNull();
    }
}