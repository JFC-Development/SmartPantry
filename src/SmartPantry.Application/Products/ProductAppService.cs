using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using SmartPantry.Domain.Products;

namespace SmartPantry.Products;

/* 
 * DECISIÓN TEMPORAL (TP05): Se habilita acceso anónimo temporalmente para 
 * verificar la operación en Swagger. No representa la política final de 
 * autorización y será revisado cuando se incorpore seguridad/login.
 */
[AllowAnonymous]
public class ProductAppService : ApplicationService, IProductAppService
{
    private readonly IRepository<Product, Guid> _repository;

    public ProductAppService(IRepository<Product, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto input)
    {
        var entity = new Product(
            Guid.NewGuid(),
            input.CodigoBarras,
            input.Nombre,
            input.Marca,
            input.Ingredientes,
            input.Alergenos
        );

        await _repository.InsertAsync(entity);

        return new ProductDto
        {
            Id = entity.Id,
            CodigoBarras = entity.CodigoBarras,
            Nombre = entity.Nombre,
            Marca = entity.Marca,
            Ingredientes = entity.Ingredientes,
            Alergenos = entity.Alergenos
        };
    }

    public async Task<ProductDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        
        return new ProductDto
        {
            Id = entity.Id,
            CodigoBarras = entity.CodigoBarras,
            Nombre = entity.Nombre,
            Marca = entity.Marca,
            Ingredientes = entity.Ingredientes,
            Alergenos = entity.Alergenos
        };
    }
}