using System;
using System.Threading.Tasks;
using Shouldly;
using SmartPantry.Despensa;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace SmartPantry.EntityFrameworkCore.Despensa;

[Collection(SmartPantryTestConsts.CollectionDefinitionName)]
public class GestorVencimientosDominioServicio_Tests : SmartPantryEntityFrameworkCoreTestBase
{
    private readonly GestorVencimientosDominioServicio _gestorVencimientos;
    private readonly IRepository<ItemDespensa, Guid> _itemRepository;
    private readonly IRepository<AdvertenciaItemDespensa, Guid> _advertenciaRepository;

    public GestorVencimientosDominioServicio_Tests()
    {
        _gestorVencimientos = GetRequiredService<GestorVencimientosDominioServicio>();
        _itemRepository = GetRequiredService<IRepository<ItemDespensa, Guid>>();
        _advertenciaRepository = GetRequiredService<IRepository<AdvertenciaItemDespensa, Guid>>();
    }

    [Fact]
    public async Task Debe_Crear_Advertencia_Para_Item_Vencido()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var item = new ItemDespensa(
            itemId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "EnStock",
            DateTime.Now.AddDays(-5),
            1,
            "unidad"
        );

        await _itemRepository.InsertAsync(item, autoSave: true);

        // Act
        await _gestorVencimientos.ProcesarVencimientosAsync();

        // Assert
        var advertencias = await _advertenciaRepository.GetListAsync(a => a.ItemDespensaId == itemId);
        advertencias.ShouldContain(a => a.TipoAdvertencia == "Vencido" && a.Activa);
    }

    [Fact]
    public async Task Debe_Crear_Advertencia_Para_Item_Proximo_A_Vencer()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var item = new ItemDespensa(
            itemId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "EnStock",
            DateTime.Now.AddDays(2),
            1,
            "unidad"
        );

        await _itemRepository.InsertAsync(item, autoSave: true);

        // Act
        await _gestorVencimientos.ProcesarVencimientosAsync();

        // Assert
        var advertencias = await _advertenciaRepository.GetListAsync(a => a.ItemDespensaId == itemId);
        advertencias.ShouldContain(a => a.TipoAdvertencia == "ProximoAVencer" && a.Activa);
    }
}
