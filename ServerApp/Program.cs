using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Memory;
using ServerApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services for OpenAPI/Swagger, CORS, in-memory caching, and response compression
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseResponseCompression();
app.UseCors("AllowBlazorClient");

const string ProductListCacheKey = "inventoryhub_productlist";

// Static seed dataset with nested Category objects
static Product[] GetSeedProducts() =>
[
    new Product(1, "Laptop", 1200.50, 25, new Category(101, "Electronics")),
    new Product(2, "Headphones", 50.00, 100, new Category(102, "Accessories")),
    new Product(3, "Mechanical Keyboard", 110.00, 45, new Category(102, "Accessories")),
    new Product(4, "27-inch 4K Monitor", 399.99, 18, new Category(101, "Electronics")),
    new Product(5, "USB-C Docking Station", 89.95, 60, new Category(103, "Peripherals"))
];

app.MapGet("/api/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }))
   .WithName("HealthCheck")
   .WithOpenApi();

// Optimized product list endpoint with server-side IMemoryCache
app.MapGet("/api/productlist", (IMemoryCache cache, IConfiguration config) =>
{
    if (!cache.TryGetValue(ProductListCacheKey, out Product[]? products))
    {
        products = GetSeedProducts();

        var cacheMinutes = config.GetValue("ApiSettings:CacheDurationMinutes", 5);
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(cacheMinutes))
            .SetSlidingExpiration(TimeSpan.FromMinutes(2));

        cache.Set(ProductListCacheKey, products, cacheOptions);
    }

    return Results.Ok(products);
})
.WithName("GetProductList")
.WithOpenApi();

app.MapGet("/api/products/{id:int}", (int id, IMemoryCache cache) =>
{
    var products = cache.GetOrCreate(ProductListCacheKey, entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
        return GetSeedProducts();
    });

    var product = products?.FirstOrDefault(p => p.Id == id);
    return product is not null
        ? Results.Ok(product)
        : Results.NotFound(new { error = $"Product with ID {id} was not found." });
})
.WithName("GetProductById")
.WithOpenApi();

app.Run();
