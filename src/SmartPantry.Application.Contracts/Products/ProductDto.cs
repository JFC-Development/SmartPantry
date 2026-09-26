using System;
using Volo.Abp.Application.Dtos;

namespace SmartPantry.Products;

public class ProductDto : EntityDto<Guid>
{
    public string CodigoBarras { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Marca { get; set; }
    public string? Ingredientes { get; set; }
    public string? Alergenos { get; set; }
}