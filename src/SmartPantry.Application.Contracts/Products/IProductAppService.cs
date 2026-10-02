using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartPantry.Products;

public interface IProductAppService :
    ICrudAppService<
        ProductDto,                      // lo que se devuelve
        Guid,                            // tipo del Id
        PagedAndSortedResultRequestDto,  // entrada de la lista (paginación y orden)
        CreateProductDto,                // entrada de creación (ya existe)
        UpdateProductDto>                // entrada de modificación (NUEVO, hay que crearlo)
{
}