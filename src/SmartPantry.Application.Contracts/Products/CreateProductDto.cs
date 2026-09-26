using System.ComponentModel.DataAnnotations;
using SmartPantry.Domain.Products; 

namespace SmartPantry.Products;

public class CreateProductDto
{
    [Required]
    [StringLength(ProductConsts.MaxCodigoBarrasLength)]
    public string CodigoBarras { get; set; } = null!;

    [Required]
    [StringLength(ProductConsts.MaxNombreLength)]
    public string Nombre { get; set; } = null!;

    [StringLength(ProductConsts.MaxMarcaLength)]
    public string? Marca { get; set; }

    [StringLength(ProductConsts.MaxIngredientesLength)]
    public string? Ingredientes { get; set; }

    [StringLength(ProductConsts.MaxAlergenosLength)]
    public string? Alergenos { get; set; }
}