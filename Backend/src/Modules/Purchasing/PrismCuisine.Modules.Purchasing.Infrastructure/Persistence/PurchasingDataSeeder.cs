using Microsoft.EntityFrameworkCore;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Purchasing.Domain.Entities;

namespace PrismCuisine.Modules.Purchasing.Infrastructure.Persistence;

public interface IPurchasingDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}

internal sealed class PurchasingDataSeeder(PrismCuisineDbContext db) : IPurchasingDataSeeder
{
    private static readonly SupplierSeed[] Suppliers =
    [
        new("NCC-RAU", "Hợp tác xã Rau sạch Đà Lạt", "0901000001", "rausach@example.com", "Chợ đầu mối Hóc Môn"),
        new("NCC-THIT", "Công ty Thực phẩm An Bình", "0901000002", "anbinh@example.com", "KCN Bình Chánh"),
        new("NCC-HAISAN", "Hải sản Nha Trang Fresh", "0901000003", "fresh@example.com", "Chợ Bình Đông"),
        new("NCC-DOUONG", "Phân phối Coca & nước giải khát", "0901000004", "coca@example.com", "Quận 7, TP.HCM"),
        new("NCC-GIAVI", "Gia vị Việt Nam Foods", "0901000005", "giaivi@example.com", "Bình Thạnh"),
        new("NCC-BAOBI", "Bao bì Xanh Packaging", "0901000006", "pack@example.com", "Thủ Đức")
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        foreach (var seed in Suppliers)
        {
            var exists = await db.Suppliers.AnyAsync(s => s.Code == seed.Code, cancellationToken);
            if (exists)
            {
                continue;
            }

            var supplier = Supplier.Create(
                seed.Code,
                seed.Name,
                seed.Phone,
                seed.Email,
                address: seed.Address,
                taxCode: seed.TaxCode);

            db.Suppliers.Add(supplier);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private sealed record SupplierSeed(
        string Code,
        string Name,
        string? Phone,
        string? Email,
        string? Address,
        string? TaxCode = null);
}
