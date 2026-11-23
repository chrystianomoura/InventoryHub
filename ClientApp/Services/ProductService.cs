using System.Net.Http.Json;
using System.Text.Json;
using ClientApp.Models;

namespace ClientApp.Services;

public sealed class ProductService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

    private readonly HttpClient _http;
    private Product[]? _cachedProducts;
    private DateTime _cacheExpiresAt = DateTime.MinValue;

    public ProductService(HttpClient http)
    {
        _http = http;
    }

    public bool IsCacheValid => _cachedProducts is not null && DateTime.UtcNow < _cacheExpiresAt;

    public async Task<Product[]> GetProductsAsync(bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        if (!forceRefresh && IsCacheValid)
        {
            return _cachedProducts!;
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(10));

        var response = await _http.GetAsync("api/productlist", timeoutCts.Token);
        response.EnsureSuccessStatusCode();

        var products = await response.Content.ReadFromJsonAsync<Product[]>(JsonOptions, timeoutCts.Token)
                       ?? Array.Empty<Product>();

        _cachedProducts = products;
        _cacheExpiresAt = DateTime.UtcNow.Add(CacheDuration);

        return products;
    }

    public void InvalidateCache()
    {
        _cachedProducts = null;
        _cacheExpiresAt = DateTime.MinValue;
    }
}
