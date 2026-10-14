using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using SmartPantry.Domain.Products;
using Volo.Abp.Application.Dtos;

namespace SmartPantry.Products;

/* 
 * DECISIÓN TEMPORAL (TP05): Se habilita acceso anónimo temporalmente para 
 * verificar la operación en Swagger. No representa la política final de 
 * autorización y será revisado cuando se incorpore seguridad/login.
 */
[AllowAnonymous]
public class ProductAppService :
    CrudAppService<
        Product,                         // entidad
        ProductDto,                      // DTO de salida
        Guid,                            // tipo del Id
        PagedAndSortedResultRequestDto,  // entrada de la lista
        CreateProductDto,                // entrada de creación
        UpdateProductDto>,               // entrada de modificación
    IProductAppService
{
    public ProductAppService(IRepository<Product, Guid> repository)
        : base(repository)
    {
    }

        // Creación: el servicio base inserta, pero la entidad se construye con el
    // constructor del agregado para que valide y normalice.
    
    protected override Task<Product> MapToEntityAsync(CreateProductDto input)
    {
        var entity = new Product(
            GuidGenerator.Create(),
            input.CodigoBarras,
            input.Nombre,
            input.Marca,
            input.Ingredientes,
            input.Alergenos
        );

        return Task.FromResult(entity);
    }

    // Modificación: se delega en Update(), que valida todo antes de asignar.
    protected override Task MapToEntityAsync(UpdateProductDto input, Product entity)
    {
        entity.Update(
            input.CodigoBarras,
            input.Nombre,
            input.Marca,
            input.Ingredientes,
            input.Alergenos
        );

        return Task.CompletedTask;
    }

    // Salida: entidad -> DTO. Es el único lugar donde se decide qué se expone.
    protected override Task<ProductDto> MapToGetOutputDtoAsync(Product entity)
    {
        return Task.FromResult(new ProductDto
        {
            Id = entity.Id,
            CodigoBarras = entity.CodigoBarras,
            Nombre = entity.Nombre,
            Marca = entity.Marca,
            Ingredientes = entity.Ingredientes,
            Alergenos = entity.Alergenos
        });
    }
}