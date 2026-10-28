using System.Threading.Tasks;
using Volo.Abp.Modularity;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Xunit;

namespace SmartPantry.ExternalProducts;

public class ExternalProductAppService_Tests
{
    private readonly IExternalProductCatalogClient _catalogClient;
    private readonly IExternalProductAppService _sut;

    public ExternalProductAppService_Tests()
    {
        _catalogClient = Substitute.For<IExternalProductCatalogClient>();
        _sut = new ExternalProductAppService(_catalogClient);
    }

    [Fact]
    public async Task Should_Return_Found_When_Product_Exists()
    {
        _catalogClient.GetByBarcodeAsync("3017620422003")
            .Returns(new ExternalProductDto
            {
                Barcode = "3017620422003",
                Name = "Nutella",
                Brand = "Ferrero",
                Quantity = "400 g"
            });

        var result = await _sut.GetByBarcodeAsync(new GetExternalProductInput { Barcode = "3017620422003" });

        result.Status.ShouldBe(ExternalProductStatus.Found);
        result.Name.ShouldBe("Nutella");
    }

    [Fact]
    public async Task Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        _catalogClient.GetByBarcodeAsync("0000000000000").Returns((ExternalProductDto?)null);

        var result = await _sut.GetByBarcodeAsync(new GetExternalProductInput { Barcode = "0000000000000" });

        result.Status.ShouldBe(ExternalProductStatus.NotFound);
    }

    [Fact]
    public async Task Should_Return_Found_With_Null_Fields_When_Data_Is_Missing()
    {
        _catalogClient.GetByBarcodeAsync("12345678")
            .Returns(new ExternalProductDto { Barcode = "12345678" });

        var result = await _sut.GetByBarcodeAsync(new GetExternalProductInput { Barcode = "12345678" });

        result.Status.ShouldBe(ExternalProductStatus.Found);
        result.Name.ShouldBeNull();
        result.Brand.ShouldBeNull();
        result.Quantity.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Return_RateLimited_When_Client_Is_Rate_Limited()
    {
        _catalogClient.GetByBarcodeAsync("3017620422003")
            .Throws(new ExternalCatalogRateLimitException());

        var result = await _sut.GetByBarcodeAsync(new GetExternalProductInput { Barcode = "3017620422003" });

        result.Status.ShouldBe(ExternalProductStatus.RateLimited);
    }

    [Fact]
    public async Task Should_Return_Unavailable_When_Provider_Is_Down()
    {
        _catalogClient.GetByBarcodeAsync("3017620422003")
            .Throws(new ExternalCatalogUnavailableException());

        var result = await _sut.GetByBarcodeAsync(new GetExternalProductInput { Barcode = "3017620422003" });

        result.Status.ShouldBe(ExternalProductStatus.Unavailable);
    }
}