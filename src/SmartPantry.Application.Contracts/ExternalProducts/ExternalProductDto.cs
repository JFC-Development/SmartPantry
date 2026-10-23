using System;

namespace SmartPantry.ExternalProducts;

public enum ExternalProductStatus
{
    Found,
    NotFound,
    RateLimited,
    Unavailable
}

public class ExternalProductResultDto
{
    public ExternalProductStatus Status { get; set; }

    public string? Barcode { get; set; }
    public string? Name { get; set; }
    public string? Brand { get; set; }
    public string? Quantity { get; set; }
}