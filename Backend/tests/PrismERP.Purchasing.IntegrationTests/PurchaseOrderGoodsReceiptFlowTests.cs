using Microsoft.Extensions.DependencyInjection;
using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Inventory.Domain.Enums;
using PrismERP.Modules.Purchasing.Application.GoodsReceipts;
using PrismERP.Modules.Purchasing.Application.PurchaseOrders;
using PrismERP.Modules.Purchasing.Domain.Enums;

namespace PrismERP.Purchasing.IntegrationTests;

public sealed class PurchaseOrderGoodsReceiptFlowTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    [Fact]
    public async Task Approve_CreatesApprovedPurchaseOrder()
    {
        await fixture.ResetDatabaseAsync();
        var seed = await fixture.SeedMasterDataAsync();
        var purchaseOrder = await fixture.CreatePurchaseOrderAsync(seed, quantity: 10m);

        await ApprovePurchaseOrderAsync(purchaseOrder.Id);

        var approved = await fixture.GetPurchaseOrderAsync(purchaseOrder.Id);
        var balance = await fixture.GetBalanceByProductWarehouseAsync(seed.ProductId, seed.WarehouseId);

        Assert.NotNull(approved);
        Assert.Equal(PurchaseOrderStatus.Approved.ToString(), approved.Status);
        Assert.Equal(10m, Assert.Single(approved.Lines).QuantityOrdered);
        Assert.Equal(0m, Assert.Single(approved.Lines).QuantityReceived);
        Assert.Null(balance);
    }

    [Fact]
    public async Task Post_PartialReceipt_KeepsPurchaseOrderPartial()
    {
        await fixture.ResetDatabaseAsync();
        var seed = await fixture.SeedMasterDataAsync();
        var purchaseOrder = await fixture.CreatePurchaseOrderAsync(seed, quantity: 10m, unitPrice: 40m);
        await ApprovePurchaseOrderAsync(purchaseOrder.Id);

        var approved = await fixture.GetPurchaseOrderAsync(purchaseOrder.Id);
        var lineId = Assert.Single(approved!.Lines).Id;
        var goodsReceipt = await fixture.CreateGoodsReceiptAsync(purchaseOrder.Id, lineId, quantity: 4m);

        await PostGoodsReceiptAsync(goodsReceipt.Id);

        var postedPo = await fixture.GetPurchaseOrderAsync(purchaseOrder.Id);
        var postedGr = await fixture.GetGoodsReceiptAsync(goodsReceipt.Id);
        var balance = await fixture.GetBalanceByProductWarehouseAsync(seed.ProductId, seed.WarehouseId);
        var receipts = await fixture.GetMovementsAsync(balance!.Id, InventoryMovementType.Receipt);

        Assert.NotNull(postedPo);
        Assert.Equal(PurchaseOrderStatus.PartiallyReceived.ToString(), postedPo.Status);
        Assert.Equal(PurchaseOrderInvoicingStatus.NotInvoiced.ToString(), postedPo.InvoiceStatus);
        Assert.Equal(4m, Assert.Single(postedPo.Lines).QuantityReceived);
        Assert.Equal(6m, Assert.Single(postedPo.Lines).QuantityRemaining);

        Assert.NotNull(postedGr);
        Assert.Equal(GoodsReceiptStatus.Posted.ToString(), postedGr.Status);

        Assert.Equal(4m, balance.QuantityOnHand);
        var receiptMovement = Assert.Single(receipts);
        Assert.Equal(4m, receiptMovement.Quantity);
        Assert.Equal(40m, receiptMovement.UnitCost);
        Assert.Equal(goodsReceipt.ReceiptNumber, receiptMovement.Reference);
        Assert.Equal(lineId, receiptMovement.ReferenceId);

        var layers = await fixture.GetCostLayersAsync(balance.Id);
        var layer = Assert.Single(layers);
        Assert.Equal(4m, layer.QuantityReceived);
        Assert.Equal(4m, layer.QuantityRemaining);
        Assert.Equal(40m, layer.UnitCost);
    }

    [Fact]
    public async Task Post_FullReceipt_MarksPurchaseOrderReceived()
    {
        await fixture.ResetDatabaseAsync();
        var seed = await fixture.SeedMasterDataAsync();
        var purchaseOrder = await fixture.CreatePurchaseOrderAsync(seed, quantity: 10m);
        await ApprovePurchaseOrderAsync(purchaseOrder.Id);

        var approved = await fixture.GetPurchaseOrderAsync(purchaseOrder.Id);
        var lineId = Assert.Single(approved!.Lines).Id;
        var goodsReceipt = await fixture.CreateGoodsReceiptAsync(purchaseOrder.Id, lineId, quantity: 10m);

        await PostGoodsReceiptAsync(goodsReceipt.Id);

        var postedPo = await fixture.GetPurchaseOrderAsync(purchaseOrder.Id);
        var balance = await fixture.GetBalanceByProductWarehouseAsync(seed.ProductId, seed.WarehouseId);

        Assert.NotNull(postedPo);
        Assert.Equal(PurchaseOrderStatus.Received.ToString(), postedPo.Status);
        Assert.Equal(PurchaseOrderInvoicingStatus.NotInvoiced.ToString(), postedPo.InvoiceStatus);
        Assert.Equal(10m, Assert.Single(postedPo.Lines).QuantityReceived);
        Assert.Equal(0m, Assert.Single(postedPo.Lines).QuantityRemaining);
        Assert.Equal(10m, balance!.QuantityOnHand);
    }

    [Fact]
    public async Task Post_SecondGoodsReceiptCompletesPartialPurchaseOrder()
    {
        await fixture.ResetDatabaseAsync();
        var seed = await fixture.SeedMasterDataAsync();
        var purchaseOrder = await fixture.CreatePurchaseOrderAsync(seed, quantity: 10m);
        await ApprovePurchaseOrderAsync(purchaseOrder.Id);

        var approved = await fixture.GetPurchaseOrderAsync(purchaseOrder.Id);
        var lineId = Assert.Single(approved!.Lines).Id;

        var firstReceipt = await fixture.CreateGoodsReceiptAsync(purchaseOrder.Id, lineId, quantity: 4m);
        await PostGoodsReceiptAsync(firstReceipt.Id);

        var secondReceipt = await fixture.CreateGoodsReceiptAsync(purchaseOrder.Id, lineId, quantity: 6m);
        await PostGoodsReceiptAsync(secondReceipt.Id);

        var postedPo = await fixture.GetPurchaseOrderAsync(purchaseOrder.Id);
        var balance = await fixture.GetBalanceByProductWarehouseAsync(seed.ProductId, seed.WarehouseId);
        var receipts = await fixture.GetMovementsAsync(balance!.Id, InventoryMovementType.Receipt);

        Assert.NotNull(postedPo);
        Assert.Equal(PurchaseOrderStatus.Received.ToString(), postedPo.Status);
        Assert.Equal(10m, Assert.Single(postedPo.Lines).QuantityReceived);
        Assert.Equal(10m, balance.QuantityOnHand);
        Assert.Equal(2, receipts.Count);
        Assert.Equal(10m, receipts.Sum(r => r.Quantity));
    }

    [Fact]
    public async Task Post_WhenQuantityExceedsRemaining_Throws()
    {
        await fixture.ResetDatabaseAsync();
        var seed = await fixture.SeedMasterDataAsync();
        var purchaseOrder = await fixture.CreatePurchaseOrderAsync(seed, quantity: 10m);
        await ApprovePurchaseOrderAsync(purchaseOrder.Id);

        var approved = await fixture.GetPurchaseOrderAsync(purchaseOrder.Id);
        var lineId = Assert.Single(approved!.Lines).Id;

        var firstReceipt = await fixture.CreateGoodsReceiptAsync(purchaseOrder.Id, lineId, quantity: 6m);
        var secondReceipt = await fixture.CreateGoodsReceiptAsync(purchaseOrder.Id, lineId, quantity: 6m);
        await PostGoodsReceiptAsync(firstReceipt.Id);

        await Assert.ThrowsAsync<BusinessException>(() => PostGoodsReceiptAsync(secondReceipt.Id));

        var reloadedPo = await fixture.GetPurchaseOrderAsync(purchaseOrder.Id);
        var balance = await fixture.GetBalanceByProductWarehouseAsync(seed.ProductId, seed.WarehouseId);

        Assert.Equal(6m, Assert.Single(reloadedPo!.Lines).QuantityReceived);
        Assert.Equal(6m, balance!.QuantityOnHand);
    }

    [Fact]
    public async Task Cancel_PostedGoodsReceipt_ReversesInventoryAndRollsBackPurchaseOrder()
    {
        await fixture.ResetDatabaseAsync();
        var seed = await fixture.SeedMasterDataAsync();
        var purchaseOrder = await fixture.CreatePurchaseOrderAsync(seed, quantity: 10m, unitPrice: 35m);
        await ApprovePurchaseOrderAsync(purchaseOrder.Id);

        var approved = await fixture.GetPurchaseOrderAsync(purchaseOrder.Id);
        var lineId = Assert.Single(approved!.Lines).Id;
        var goodsReceipt = await fixture.CreateGoodsReceiptAsync(purchaseOrder.Id, lineId, quantity: 4m);
        await PostGoodsReceiptAsync(goodsReceipt.Id);

        await CancelGoodsReceiptAsync(goodsReceipt.Id);

        var cancelledPo = await fixture.GetPurchaseOrderAsync(purchaseOrder.Id);
        var cancelledGr = await fixture.GetGoodsReceiptAsync(goodsReceipt.Id);
        var balance = await fixture.GetBalanceByProductWarehouseAsync(seed.ProductId, seed.WarehouseId);
        var receipts = await fixture.GetMovementsAsync(balance!.Id, InventoryMovementType.Receipt);
        var returns = await fixture.GetMovementsAsync(balance.Id, InventoryMovementType.Return);

        Assert.NotNull(cancelledPo);
        Assert.Equal(PurchaseOrderStatus.Approved.ToString(), cancelledPo.Status);
        Assert.Equal(PurchaseOrderInvoicingStatus.NotInvoiced.ToString(), cancelledPo.InvoiceStatus);
        Assert.Equal(0m, Assert.Single(cancelledPo.Lines).QuantityReceived);
        Assert.Equal(10m, Assert.Single(cancelledPo.Lines).QuantityRemaining);

        Assert.NotNull(cancelledGr);
        Assert.Equal(GoodsReceiptStatus.Cancelled.ToString(), cancelledGr.Status);

        Assert.Equal(0m, balance.QuantityOnHand);
        Assert.Single(receipts);
        var returnMovement = Assert.Single(returns);
        Assert.Equal(4m, returnMovement.Quantity);
        Assert.Equal(lineId, returnMovement.ReferenceId);
        Assert.Equal(goodsReceipt.ReceiptNumber, returnMovement.Reference);

        var layers = await fixture.GetCostLayersAsync(balance.Id);
        var layer = Assert.Single(layers);
        Assert.Equal(0m, layer.QuantityRemaining);
    }

    [Fact]
    public async Task CancelPurchaseOrder_WithDraftGoodsReceipts_DeletesReceipts()
    {
        await fixture.ResetDatabaseAsync();
        var seed = await fixture.SeedMasterDataAsync();
        var purchaseOrder = await fixture.CreatePurchaseOrderAsync(seed, quantity: 10m);
        await ApprovePurchaseOrderAsync(purchaseOrder.Id);

        var approved = await fixture.GetPurchaseOrderAsync(purchaseOrder.Id);
        var lineId = Assert.Single(approved!.Lines).Id;
        var draftReceipt = await fixture.CreateGoodsReceiptAsync(purchaseOrder.Id, lineId, quantity: 4m);

        await CancelPurchaseOrderAsync(purchaseOrder.Id);

        var cancelledPo = await fixture.GetPurchaseOrderAsync(purchaseOrder.Id);
        var reloadedGr = await fixture.GetGoodsReceiptAsync(draftReceipt.Id);

        Assert.NotNull(cancelledPo);
        Assert.Equal(PurchaseOrderStatus.Cancelled.ToString(), cancelledPo.Status);
        Assert.Null(reloadedGr);
    }

    [Fact]
    public async Task ConcurrentApprove_ForSamePurchaseOrder_AllowsOnlyOneApproval()
    {
        await fixture.ResetDatabaseAsync();
        var seed = await fixture.SeedMasterDataAsync();
        var purchaseOrder = await fixture.CreatePurchaseOrderAsync(seed, quantity: 6m);

        var results = await RunConcurrently(
            () => ApprovePurchaseOrderAsync(purchaseOrder.Id),
            () => ApprovePurchaseOrderAsync(purchaseOrder.Id));

        Assert.Equal(1, results.Count(r => r.Success));
        var failure = Assert.Single(results, r => !r.Success);
        Assert.IsType<ConflictException>(failure.Exception);

        var approved = await fixture.GetPurchaseOrderAsync(purchaseOrder.Id);
        Assert.Equal(PurchaseOrderStatus.Approved.ToString(), approved!.Status);
    }

    [Fact]
    public async Task ConcurrentPost_ForTwoGoodsReceiptsOnSamePoLine_DoesNotOverReceive()
    {
        await fixture.ResetDatabaseAsync();
        var seed = await fixture.SeedMasterDataAsync();
        var purchaseOrder = await fixture.CreatePurchaseOrderAsync(seed, quantity: 10m);
        await ApprovePurchaseOrderAsync(purchaseOrder.Id);

        var approved = await fixture.GetPurchaseOrderAsync(purchaseOrder.Id);
        var lineId = Assert.Single(approved!.Lines).Id;
        var firstReceipt = await fixture.CreateGoodsReceiptAsync(purchaseOrder.Id, lineId, quantity: 6m);
        var secondReceipt = await fixture.CreateGoodsReceiptAsync(purchaseOrder.Id, lineId, quantity: 6m);

        var results = await RunConcurrently(
            () => PostGoodsReceiptAsync(firstReceipt.Id),
            () => PostGoodsReceiptAsync(secondReceipt.Id));

        Assert.Equal(1, results.Count(r => r.Success));
        var failure = Assert.Single(results, r => !r.Success);
        Assert.IsType<BusinessException>(failure.Exception);

        var postedPo = await fixture.GetPurchaseOrderAsync(purchaseOrder.Id);
        var balance = await fixture.GetBalanceByProductWarehouseAsync(seed.ProductId, seed.WarehouseId);
        var receipts = await fixture.GetMovementsAsync(balance!.Id, InventoryMovementType.Receipt);

        Assert.Equal(PurchaseOrderStatus.PartiallyReceived.ToString(), postedPo!.Status);
        Assert.Equal(6m, Assert.Single(postedPo.Lines).QuantityReceived);
        Assert.Equal(6m, balance.QuantityOnHand);
        Assert.Equal(6m, receipts.Sum(r => r.Quantity));
    }

    [Fact]
    public async Task Cancel_Twice_ThrowsConflict()
    {
        await fixture.ResetDatabaseAsync();
        var seed = await fixture.SeedMasterDataAsync();
        var purchaseOrder = await fixture.CreatePurchaseOrderAsync(seed, quantity: 5m);
        await ApprovePurchaseOrderAsync(purchaseOrder.Id);

        var approved = await fixture.GetPurchaseOrderAsync(purchaseOrder.Id);
        var lineId = Assert.Single(approved!.Lines).Id;
        var goodsReceipt = await fixture.CreateGoodsReceiptAsync(purchaseOrder.Id, lineId, quantity: 5m);
        await PostGoodsReceiptAsync(goodsReceipt.Id);

        await CancelGoodsReceiptAsync(goodsReceipt.Id);

        await Assert.ThrowsAsync<ConflictException>(() => CancelGoodsReceiptAsync(goodsReceipt.Id));

        var balance = await fixture.GetBalanceByProductWarehouseAsync(seed.ProductId, seed.WarehouseId);
        Assert.Equal(0m, balance!.QuantityOnHand);
    }

    private Task ApprovePurchaseOrderAsync(int purchaseOrderId)
        => fixture.ExecuteInScopeAsync(sp =>
            sp.GetRequiredService<IPurchaseOrderService>().ApproveAsync(purchaseOrderId));

    private Task CancelPurchaseOrderAsync(int purchaseOrderId)
        => fixture.ExecuteInScopeAsync(sp =>
            sp.GetRequiredService<IPurchaseOrderService>().CancelAsync(purchaseOrderId));

    private Task PostGoodsReceiptAsync(int goodsReceiptId)
        => fixture.ExecuteInScopeAsync(sp =>
            sp.GetRequiredService<IGoodsReceiptService>().PostAsync(goodsReceiptId));

    private Task CancelGoodsReceiptAsync(int goodsReceiptId)
        => fixture.ExecuteInScopeAsync(sp =>
            sp.GetRequiredService<IGoodsReceiptService>().CancelAsync(goodsReceiptId));

    private static async Task<IReadOnlyList<RunResult>> RunConcurrently(
        Func<Task> first,
        Func<Task> second)
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var firstTask = CaptureAfterGateAsync(gate.Task, first);
        var secondTask = CaptureAfterGateAsync(gate.Task, second);
        gate.SetResult();

        return await Task.WhenAll(firstTask, secondTask);
    }

    private static async Task<RunResult> CaptureAfterGateAsync(Task gate, Func<Task> action)
    {
        await gate;
        try
        {
            await action();
            return new RunResult(true, null);
        }
        catch (Exception ex)
        {
            return new RunResult(false, ex);
        }
    }

    private sealed record RunResult(bool Success, Exception? Exception);
}
