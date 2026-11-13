namespace SmartPantry.ExternalProducts;

public class ExternalProductDto
{
    public string Barcode { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Brand { get; set; }
    public string? Quantity { get; set; }
}