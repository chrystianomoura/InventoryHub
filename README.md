# InventoryHub

**InventoryHub** is a full-stack .NET 8 inventory management application that integrates a **Blazor WebAssembly** front-end (`ClientApp`) with an **ASP.NET Core Minimal API** back-end (`ServerApp`).

## Project Structure

```text
InventoryHub/
├── InventoryHub.sln
├── ClientApp/                 # Blazor WebAssembly front-end
│   ├── Models/Product.cs      # Strongly-typed Product & nested Category models
│   ├── Services/ProductService.cs  # API client with caching, timeouts & JSON options
│   ├── Pages/FetchProducts.razor   # Product inventory view with error handling
│   └── Program.cs
└── ServerApp/                 # ASP.NET Core Minimal API back-end
    ├── Models/Product.cs      # Product & Category record definitions
    ├── Program.cs             # Minimal API endpoints, CORS, IMemoryCache & Gzip compression
    └── appsettings.json
```

## API Endpoints (`ServerApp`)

- `GET /api/productlist` — Returns the product catalog as structured JSON, including nested `category` objects. Cached server-side with `IMemoryCache`.
- `GET /api/products/{id}` — Returns a single product by ID or `404 Not Found`.
- `GET /api/health` — Simple health-check endpoint.

### Sample JSON Response (`/api/productlist`)

```json
[
  {
    "id": 1,
    "name": "Laptop",
    "price": 1200.5,
    "stock": 25,
    "category": {
      "id": 101,
      "name": "Electronics"
    }
  }
]
```

## Running Locally

1. **Start the back-end API:**
   ```bash
   cd ServerApp
   dotnet run --urls http://localhost:5000
   ```
2. **Start the Blazor WebAssembly front-end:**
   ```bash
   cd ClientApp
   dotnet run
   ```
3. Navigate to `/fetchproducts` in the browser to view the product inventory.

---

## Reflective Summary: How Microsoft Copilot Assisted

1. **Generating and Refining Integration Code (Activity 1):**
   Microsoft Copilot accelerated scaffolding the `ClientApp` Blazor WebAssembly `HttpClient` integration and the `ServerApp` Minimal API `/api/productlist` route. It helped structure the asynchronous `GetProductsAsync` call and bind the returned data cleanly into the `FetchProducts.razor` UI.
2. **Debugging and Resolving Integration Issues (Activity 2):**
   Copilot helped identify and fix cross-origin resource sharing (CORS) issues by configuring the `AllowBlazorClient` CORS policy in `ServerApp/Program.cs`, aligned the API route and `BaseAddress`, and added granular exception handling (`OperationCanceledException`, `HttpRequestException`, `JsonException`) plus request timeouts in `ProductService` and `FetchProducts.razor`.
3. **Structuring JSON Responses (Activity 3):**
   Copilot assisted in expanding the flat product payload into a structured JSON contract containing a nested `Category` object (`id`, `name`) and matching C# models with `[JsonPropertyName]` attributes and case-insensitive `JsonSerializerOptions`.
4. **Optimizing Performance (Activity 4):**
   Copilot recommended and helped implement server-side `IMemoryCache` with sliding/absolute expiration and Gzip response compression in `ServerApp`, alongside client-side in-memory caching (`ProductService`), static `JsonSerializerOptions` reuse, and `CancellationTokenSource` disposal in `FetchProducts.razor` to eliminate redundant network calls and allocations.
