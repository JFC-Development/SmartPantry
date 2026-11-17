using System;
using Volo.Abp.Application.Dtos;

namespace SmartPantry.Despensa;

public class PantryItemDto : EntityDto<Guid>
{
    public Guid ProductoId { get; set; }
    public string NombreProducto { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public DateTime? FechaVencimiento { get; set; }
    public decimal? Cantidad { get; set; }
    public string? Unidad { get; set; }
}