using System;

namespace SmartPantry.ExternalProducts;

public class ExternalCatalogRateLimitException : Exception
{
    public ExternalCatalogRateLimitException() : base("El catálogo externo limitó las consultas.") { }
}

public class ExternalCatalogUnavailableException : Exception
{
    public ExternalCatalogUnavailableException() : base("El catálogo externo no está disponible.") { }
}