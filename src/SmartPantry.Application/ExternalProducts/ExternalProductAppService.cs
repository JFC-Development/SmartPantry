using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartPantry.ExternalProducts;

public class ExternalProductAppService : ApplicationService, IExternalProductAppService
{
    private readonly IExternalProductCatalogClient _catalogClient;

    public ExternalProductAppService(IExternalProductCatalogClient catalogClient)
    {
        _catalogClient = catalogClient;
    }

    public async Task<ExternalProductResultDto> GetByBarcodeAsync(GetExternalProductInput input)
    {
        try
        {
            var product = await _catalogClient.GetByBarcodeAsync(input.Barcode);

            if (product is null)
            {
                return new ExternalProductResultDto { Status = ExternalProductStatus.NotFound, Barcode = input.Barcode };
            }

            return new ExternalProductResultDto
            {
                Status = ExternalProductStatus.Found,
                Barcode = product.Barcode,
                Name = product.Name,
                Brand = product.Brand,
                Quantity = product.Quantity
            };
        }
        catch (ExternalCatalogRateLimitException)
        {
            return new ExternalProductResultDto { Status = ExternalProductStatus.RateLimited, Barcode = input.Barcode };
        }
        catch (ExternalCatalogUnavailableException)
        {
            return new ExternalProductResultDto { Status = ExternalProductStatus.Unavailable, Barcode = input.Barcode };
        }
    }
}