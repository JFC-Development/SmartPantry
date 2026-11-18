using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using SmartPantry.Domain.Products;

namespace SmartPantry.Despensa;

[Authorize]
public class PantryItemAppService : ApplicationService, IPantryItemAppService
{
    private readonly IRepository<ItemDespensa, Guid> _itemRepository;
    private readonly IRepository<Product, Guid> _productRepository;

    public PantryItemAppService(
        IRepository<ItemDespensa, Guid> itemRepository,
        IRepository<Product, Guid> productRepository)
    {
        _itemRepository = itemRepository;
        _productRepository = productRepository;
    }

    public virtual async Task<PagedResultDto<PantryItemDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var userId = GetUserId();
        var queryable = await _itemRepository.GetQueryableAsync();

        var propios = queryable.Where(x => x.PropietarioId == userId);
        var total = propios.Count();

        var items = propios
            .OrderBy(x => x.FechaVencimiento == null)   // los sin fecha al final
            .ThenBy(x => x.FechaVencimiento)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        var dtos = await MapearAsync(items);
        return new PagedResultDto<PantryItemDto>(total, dtos);
    }

    public virtual async Task<PantryItemDto> UpdateAsync(Guid id, UpdatePantryItemDto input)
    {
        var item = await ObtenerPropioAsync(id);
        item.Actualizar(input.Cantidad, input.Unidad, input.FechaVencimiento);
        await _itemRepository.UpdateAsync(item);
        return (await MapearAsync(new List<ItemDespensa> { item })).Single();
    }

    public virtual async Task<PantryItemDto> MarkAsConsumedAsync(Guid id)
    {
        var item = await ObtenerPropioAsync(id);
        item.MarcarConsumido();
        await _itemRepository.UpdateAsync(item);
        return (await MapearAsync(new List<ItemDespensa> { item })).Single();
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var item = await ObtenerPropioAsync(id);
        await _itemRepository.DeleteAsync(item);
    }

    private Guid GetUserId()
    {
        return CurrentUser.Id ?? throw new AbpAuthorizationException();
    }
    private async Task<ItemDespensa> ObtenerPropioAsync(Guid id)
    {
        var userId = GetUserId();
        var item = await _itemRepository.FindAsync(x => x.Id == id && x.PropietarioId == userId);
        if (item == null)
        {
            throw new EntityNotFoundException(typeof(ItemDespensa), id);
        }
        return item;
    }

    private async Task<List<PantryItemDto>> MapearAsync(List<ItemDespensa> items)
    {
        var productIds = items.Select(i => i.ProductoId).Distinct().ToList();
        var productos = await _productRepository.GetListAsync(p => productIds.Contains(p.Id));
        var nombres = productos.ToDictionary(p => p.Id, p => p.Nombre);

        return items.Select(i => new PantryItemDto
        {
            Id = i.Id,
            ProductoId = i.ProductoId,
            NombreProducto = nombres.TryGetValue(i.ProductoId, out var n) ? n : string.Empty,
            Estado = i.Estado,
            FechaVencimiento = i.FechaVencimiento,
            Cantidad = i.Cantidad,
            Unidad = i.Unidad
        }).ToList();
    }
}