namespace ServerApp.Models;

public sealed record Category(int Id, string Name);

public sealed record Product(
    int Id,
    string Name,
    double Price,
    int Stock,
    Category Category
);
