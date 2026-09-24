using System.ComponentModel.DataAnnotations;

namespace SmartPantry.Products;

public class CreateProductDto
{
    [Required]
    [StringLength(ProductoConsts.MaxCodigoBarrasLength)]
    public string CodigoBarras { get; set; } = null!;

    [Required]
    [StringLength(ProductoConsts.MaxNombreLength)]
    public string Nombre { get; set; } = null!;

    [StringLength(ProductoConsts.MaxMarcaLength)]
    public string? Marca { get; set; }

    [StringLength(ProductoConsts.MaxIngredientesLength)]
    public string? Ingredientes { get; set; }

    [StringLength(ProductoConsts.MaxAlergenosLength)]
    public string? Alergenos { get; set; }
}