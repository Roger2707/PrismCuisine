namespace PrismERP.Modules.Identity.Application.Authorization;

/// <summary>
/// Permission codes seeded in IdentityDataSeeder. Policy names match these values exactly.
/// </summary>
public static class PermissionCodes
{
    // Identity
    public const string UsersRead = "users:read";
    public const string RolesRead = "roles:read";
    public const string RolesWrite = "roles:write";

    // Inventory — master data
    public const string ProductCategoryRead = "product-category:read";
    public const string ProductCategoryWrite = "product-category:write";
    public const string ProductRead = "product:read";
    public const string ProductWrite = "product:write";
    public const string WarehouseRead = "warehouse:read";
    public const string WarehouseWrite = "warehouse:write";
    public const string SupplierRead = "supplier:read";
    public const string SupplierWrite = "supplier:write";
    public const string CustomerRead = "customer:read";
    public const string CustomerWrite = "customer:write";

    // Inventory — stock
    public const string InventoryRead = "inventory:read";
    public const string InventoryAdjust = "inventory:adjust";

    // Purchasing
    public const string PurchaseRead = "purchase:read";
    public const string PurchaseWrite = "purchase:write";
    public const string PurchaseApprove = "purchase:approve";
    public const string PurchaseAmend = "purchase:amend";
    public const string PurchaseCancel = "purchase:cancel";
    public const string GoodsReceiptRead = "goods-receipt:read";
    public const string GoodsReceiptWrite = "goods-receipt:write";
    public const string GoodsReceiptPost = "goods-receipt:post";
    public const string GoodsReceiptCancel = "goods-receipt:cancel";
    public const string PurchaseInvoiceRead = "purchase-invoice:read";
    public const string PurchaseInvoiceWrite = "purchase-invoice:write";

    // Sales
    public const string SalesOrderRead = "salesorder:read";
    public const string SalesOrderWrite = "salesorder:write";
    public const string SalesOrderApprove = "salesorder:approve";
    public const string SalesOrderCancel = "salesorder:cancel";
    public const string DeliveryRead = "delivery:read";
    public const string DeliveryWrite = "delivery:write";
    public const string DeliveryPost = "delivery:post";
    public const string DeliveryCancel = "delivery:cancel";

    // Finance
    public const string InvoiceRead = "invoice:read";
    public const string InvoiceWrite = "invoice:write";
    public const string PaymentRead = "payment:read";
    public const string PaymentWrite = "payment:write";
    public const string PaymentProcess = "payment:process";

    public static IReadOnlyList<string> All { get; } =
    [
        UsersRead, RolesRead, RolesWrite,
        ProductCategoryRead, ProductCategoryWrite,
        ProductRead, ProductWrite,
        WarehouseRead, WarehouseWrite,
        SupplierRead, SupplierWrite,
        CustomerRead, CustomerWrite,
        InventoryRead, InventoryAdjust,
        PurchaseRead, PurchaseWrite, PurchaseApprove, PurchaseAmend, PurchaseCancel,
        GoodsReceiptRead, GoodsReceiptWrite, GoodsReceiptPost, GoodsReceiptCancel,
        PurchaseInvoiceRead, PurchaseInvoiceWrite,
        SalesOrderRead, SalesOrderWrite, SalesOrderApprove, SalesOrderCancel,
        DeliveryRead, DeliveryWrite, DeliveryPost, DeliveryCancel,
        InvoiceRead, InvoiceWrite,
        PaymentRead, PaymentWrite, PaymentProcess
    ];
}
