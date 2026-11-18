using System;
using System.ComponentModel.DataAnnotations;

namespace SmartPantry.Despensa;

public class UpdatePantryItemDto
{
    [Range(typeof(decimal), "0", "999999999")]
    public decimal? Cantidad { get; set; }

    [StringLength(ItemDespensaConsts.MaxUnidadLength)]
    public string? Unidad { get; set; }

    public DateTime? FechaVencimiento { get; set; }
}