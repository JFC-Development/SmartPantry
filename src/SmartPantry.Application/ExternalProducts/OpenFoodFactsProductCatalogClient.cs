using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartPantry.ExternalProducts;

public class OpenFoodFactsProductCatalogClient : IExternalProductCatalogClient
{
    private readonly HttpClient _httpClient;

    public OpenFoodFactsProductCatalogClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ExternalProductDto?> GetByBarcodeAsync(string barcode)
    {
        var url = $"api/v3/product/{barcode}?fields=code,product_name,product_name_es,brands,quantity";

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(url);
        }
        catch (TaskCanceledException)
        {
            throw new ExternalCatalogUnavailableException();
        }
        catch (HttpRequestException)
        {
            throw new ExternalCatalogUnavailableException();
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new ExternalCatalogRateLimitException();
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new ExternalCatalogUnavailableException();
        }

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var root = doc.RootElement;

        if (!root.TryGetProperty("product", out var product))
        {
            return null;
        }

        return new ExternalProductDto
        {
            Barcode = ReadText(product, "code") ?? barcode,
            Name = ReadText(product, "product_name_es") ?? ReadText(product, "product_name"),
            Brand = ReadText(product, "brands"),
            Quantity = ReadText(product, "quantity")
        };
    }

    private static string? ReadText(JsonElement element, string property)
    {
        if (element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String)
        {
            var text = value.GetString();
            return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
        }

        return null;
    }
}