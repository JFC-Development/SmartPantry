using System.ComponentModel.DataAnnotations;

namespace SmartPantry.ExternalProducts;

public class GetExternalProductInput
{
    [Required]
    [StringLength(14, MinimumLength = 8)]
    [RegularExpression(@"^\d+$", ErrorMessage = "El código de barras debe contener solo dígitos.")]
    public string Barcode { get; set; } = string.Empty;
}