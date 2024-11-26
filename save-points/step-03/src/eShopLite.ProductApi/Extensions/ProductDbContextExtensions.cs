using eShopLite.ProductData;

namespace eShopLite.ProductApi.Extensions;

public static class ProductDbContextExtensions
{
    public static void CreateDbIfNotExists(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ProductDbContext>();
        context.Database.EnsureCreated();
        DbInitializer.Initialize(context);
    }
}
