using Microsoft.Extensions.DependencyInjection;
using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Finance.Domain.Enums;
using PrismERP.Modules.Inventory.Domain.Enums;
using PrismERP.Modules.SalesOrdering.Application.Deliveries;
using PrismERP.Modules.SalesOrdering.Application.SalesOrders;
using PrismERP.Modules.SalesOrdering.Domain.Enums;

namespace PrismERP.SalesOrdering.IntegrationTests;

public sealed class SalesOrderDeliveryFlowTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    [Fact]
    public async Task Approve_WithEnoughStock_ConfirmsOrderAndCreatesReservation()
    {
        await fixture.ResetDatabaseAsync();
        var inventory = await fixture.SeedInventoryAsync((10m, 12m));
        var salesOrder = await fixture.CreateSalesOrderAsync(inventory.ProductId, inventory.ProductName, quantity: 6m);

        await fixture.ExecuteInScopeAsync(sp =>
            sp.GetRequiredService<ISalesOrderService>().ApproveAsync(salesOrder.Id));

        var approved = await fixture.GetSalesOrderAsync(salesOrder.Id);
        var reservations = await fixture.GetReservationsAsync(inventory.BalanceId);
        var balance = await fixture.GetBalanceAsync(inventory.BalanceId);

        Assert.NotNull(approved);
        Assert.Equal(SalesOrderStatus.Confirmed.ToString(), approved.Status);
        var reservation = Assert.Single(reservations);
        Assert.Equal(6m, reservation.Quantity);
        Assert.Equal(0m, reservation.FulfilledQuantity);
        Assert.Equal(InventoryReservationStatus.Active, reservation.Status);
        Assert.Equal(10m, balance.QuantityOnHand);
    }

    [Fact]
    public async Task Approve_WithInsufficientAvailableStock_ThrowsAndDoesNotReserve()
    {
        await fixture.ResetDatabaseAsync();
        var inventory = await fixture.SeedInventoryAsync((5m, 12m));
        var salesOrder = await fixture.CreateSalesOrderAsync(inventory.ProductId, inventory.ProductName, quantity: 6m);

        await Assert.ThrowsAsync<BusinessException>(() =>
            fixture.ExecuteInScopeAsync(sp =>
                sp.GetRequiredService<ISalesOrderService>().ApproveAsync(salesOrder.Id)));

        var reloaded = await fixture.GetSalesOrderAsync(salesOrder.Id);
        var reservations = await fixture.GetReservationsAsync(inventory.BalanceId);
        var balance = await fixture.GetBalanceAsync(inventory.BalanceId);

        Assert.NotNull(reloaded);
        Assert.Equal(SalesOrderStatus.Draft.ToString(), reloaded.Status);
        Assert.Empty(reservations);
        Assert.Equal(5m, balance.QuantityOnHand);
    }

    [Fact]
    public async Task Post_PartialDelivery_IssuesStockAndKeepsSalesOrderPartial()
    {
        await fixture.ResetDatabaseAsync();
        var inventory = await fixture.SeedInventoryAsync((10m, 12m));
        var salesOrder = await fixture.CreateSalesOrderAsync(inventory.ProductId, inventory.ProductName, quantity: 10m);
        await ApproveAsync(salesOrder.Id);

        var approved = await fixture.GetSalesOrderAsync(salesOrder.Id);
        var lineId = Assert.Single(approved!.Lines).Id;
        var deliveryNote = await fixture.CreateDeliveryNoteAsync(salesOrder.Id, lineId, quantityDelivered: 4m);

        await PostAsync(deliveryNote.Id);

        var postedSalesOrder = await fixture.GetSalesOrderAsync(salesOrder.Id);
        var balance = await fixture.GetBalanceAsync(inventory.BalanceId);
        var reservations = await fixture.GetReservationsAsync(inventory.BalanceId);
        var issues = await fixture.GetMovementsAsync(inventory.BalanceId, InventoryMovementType.Issue);
        var invoices = await fixture.GetInvoicesByDeliveryNoteAsync(deliveryNote.Id);

        Assert.NotNull(postedSalesOrder);
        Assert.Equal(SalesOrderStatus.PartialDelivery.ToString(), postedSalesOrder.Status);
        Assert.Equal(SalesOrderInvoicingStatus.PartiallyInvoiced.ToString(), postedSalesOrder.InvoiceStatus);
        Assert.Equal(4m, Assert.Single(postedSalesOrder.Lines).QuantityDelivered);
        Assert.Equal(6m, balance.QuantityOnHand);

        var reservation = Assert.Single(reservations);
        Assert.Equal(10m, reservation.Quantity);
        Assert.Equal(4m, reservation.FulfilledQuantity);
        Assert.Equal(InventoryReservationStatus.Active, reservation.Status);

        var issue = Assert.Single(issues);
        Assert.Equal(4m, issue.Quantity);
        Assert.Equal(deliveryNote.DeliveryNumber, issue.Reference);
        Assert.Equal(lineId, issue.ReferenceId);

        var invoice = Assert.Single(invoices);
        Assert.Equal(InvoiceStatus.Unpaid, invoice.Status);
    }

    [Fact]
    public async Task Post_FullDelivery_MarksSalesOrderDeliveredAndFullyInvoiced()
    {
        await fixture.ResetDatabaseAsync();
        var inventory = await fixture.SeedInventoryAsync((10m, 12m));
        var salesOrder = await fixture.CreateSalesOrderAsync(inventory.ProductId, inventory.ProductName, quantity: 10m);
        await ApproveAsync(salesOrder.Id);

        var approved = await fixture.GetSalesOrderAsync(salesOrder.Id);
        var lineId = Assert.Single(approved!.Lines).Id;
        var deliveryNote = await fixture.CreateDeliveryNoteAsync(salesOrder.Id, lineId, quantityDelivered: 10m);

        await PostAsync(deliveryNote.Id);

        var postedSalesOrder = await fixture.GetSalesOrderAsync(salesOrder.Id);
        var balance = await fixture.GetBalanceAsync(inventory.BalanceId);
        var reservations = await fixture.GetReservationsAsync(inventory.BalanceId);

        Assert.NotNull(postedSalesOrder);
        Assert.Equal(SalesOrderStatus.Delivered.ToString(), postedSalesOrder.Status);
        Assert.Equal(SalesOrderInvoicingStatus.FullyInvoiced.ToString(), postedSalesOrder.InvoiceStatus);
        Assert.Equal(10m, Assert.Single(postedSalesOrder.Lines).QuantityDelivered);
        Assert.Equal(0m, balance.QuantityOnHand);

        var reservation = Assert.Single(reservations);
        Assert.Equal(10m, reservation.FulfilledQuantity);
        Assert.Equal(InventoryReservationStatus.Fulfilled, reservation.Status);
    }

    [Fact]
    public async Task Post_WhenFifoRequiresTwoCostLayers_IssuesFromOldestLayersFirst()
    {
        await fixture.ResetDatabaseAsync();
        var inventory = await fixture.SeedInventoryAsync((4m, 10m), (6m, 20m));
        var salesOrder = await fixture.CreateSalesOrderAsync(inventory.ProductId, inventory.ProductName, quantity: 7m);
        await ApproveAsync(salesOrder.Id);

        var approved = await fixture.GetSalesOrderAsync(salesOrder.Id);
        var lineId = Assert.Single(approved!.Lines).Id;
        var deliveryNote = await fixture.CreateDeliveryNoteAsync(salesOrder.Id, lineId, quantityDelivered: 7m);

        await PostAsync(deliveryNote.Id);

        var layers = await fixture.GetCostLayersAsync(inventory.BalanceId);
        var issues = await fixture.GetMovementsAsync(inventory.BalanceId, InventoryMovementType.Issue);
        var balance = await fixture.GetBalanceAsync(inventory.BalanceId);

        Assert.Collection(layers,
            first =>
            {
                Assert.Equal(4m, first.QuantityReceived);
                Assert.Equal(0m, first.QuantityRemaining);
                Assert.Equal(10m, first.UnitCost);
            },
            second =>
            {
                Assert.Equal(6m, second.QuantityReceived);
                Assert.Equal(3m, second.QuantityRemaining);
                Assert.Equal(20m, second.UnitCost);
            });

        Assert.Collection(issues,
            first =>
            {
                Assert.Equal(4m, first.Quantity);
                Assert.Equal(10m, first.UnitCost);
                Assert.Equal(deliveryNote.DeliveryNumber, first.Reference);
            },
            second =>
            {
                Assert.Equal(3m, second.Quantity);
                Assert.Equal(20m, second.UnitCost);
                Assert.Equal(deliveryNote.DeliveryNumber, second.Reference);
            });

        Assert.Equal(3m, balance.QuantityOnHand);
    }

    [Fact]
    public async Task ConcurrentApprove_ForSameSalesOrder_AllowsOnlyOneApproval()
    {
        await fixture.ResetDatabaseAsync();
        var inventory = await fixture.SeedInventoryAsync((10m, 12m));
        var salesOrder = await fixture.CreateSalesOrderAsync(inventory.ProductId, inventory.ProductName, quantity: 6m);

        var results = await RunConcurrently(
            () => ApproveAsync(salesOrder.Id),
            () => ApproveAsync(salesOrder.Id));

        Assert.Equal(1, results.Count(r => r.Success));
        var failure = Assert.Single(results, r => !r.Success);
        Assert.IsType<ConflictException>(failure.Exception);

        var approved = await fixture.GetSalesOrderAsync(salesOrder.Id);
        var reservations = await fixture.GetReservationsAsync(inventory.BalanceId);
        Assert.Equal(SalesOrderStatus.Confirmed.ToString(), approved!.Status);
        Assert.Single(reservations);
    }

    [Fact]
    public async Task ConcurrentApprove_ForDifferentSalesOrdersOnSameStock_DoesNotOverReserve()
    {
        await fixture.ResetDatabaseAsync();
        var inventory = await fixture.SeedInventoryAsync((10m, 12m));
        var firstSalesOrder = await fixture.CreateSalesOrderAsync(inventory.ProductId, inventory.ProductName, quantity: 7m);
        var secondSalesOrder = await fixture.CreateSalesOrderAsync(inventory.ProductId, inventory.ProductName, quantity: 7m);

        var results = await RunConcurrently(
            () => ApproveAsync(firstSalesOrder.Id),
            () => ApproveAsync(secondSalesOrder.Id));

        Assert.Equal(1, results.Count(r => r.Success));
        var failure = Assert.Single(results, r => !r.Success);
        Assert.IsType<BusinessException>(failure.Exception);

        var reservations = await fixture.GetReservationsAsync(inventory.BalanceId);
        var activeReservation = Assert.Single(reservations);
        Assert.Equal(7m, activeReservation.Quantity);
        Assert.Equal(InventoryReservationStatus.Active, activeReservation.Status);
    }

    [Fact]
    public async Task ConcurrentPost_ForTwoDeliveryNotesOnSameReservation_DoesNotOverDeliver()
    {
        await fixture.ResetDatabaseAsync();
        var inventory = await fixture.SeedInventoryAsync((10m, 12m));
        var salesOrder = await fixture.CreateSalesOrderAsync(inventory.ProductId, inventory.ProductName, quantity: 10m);
        await ApproveAsync(salesOrder.Id);

        var approved = await fixture.GetSalesOrderAsync(salesOrder.Id);
        var lineId = Assert.Single(approved!.Lines).Id;
        var firstDeliveryNote = await fixture.CreateDeliveryNoteAsync(salesOrder.Id, lineId, quantityDelivered: 6m);
        var secondDeliveryNote = await fixture.CreateDeliveryNoteAsync(salesOrder.Id, lineId, quantityDelivered: 6m);

        var results = await RunConcurrently(
            () => PostAsync(firstDeliveryNote.Id),
            () => PostAsync(secondDeliveryNote.Id));

        Assert.Equal(1, results.Count(r => r.Success));
        var failure = Assert.Single(results, r => !r.Success);
        Assert.IsType<BusinessException>(failure.Exception);

        var postedSalesOrder = await fixture.GetSalesOrderAsync(salesOrder.Id);
        var balance = await fixture.GetBalanceAsync(inventory.BalanceId);
        var reservations = await fixture.GetReservationsAsync(inventory.BalanceId);
        var issues = await fixture.GetMovementsAsync(inventory.BalanceId, InventoryMovementType.Issue);

        Assert.Equal(SalesOrderStatus.PartialDelivery.ToString(), postedSalesOrder!.Status);
        Assert.Equal(6m, Assert.Single(postedSalesOrder.Lines).QuantityDelivered);
        Assert.Equal(4m, balance.QuantityOnHand);
        Assert.Equal(6m, issues.Sum(i => i.Quantity));

        var reservation = Assert.Single(reservations);
        Assert.Equal(6m, reservation.FulfilledQuantity);
        Assert.Equal(4m, reservation.RemainingQuantity);
        Assert.Equal(InventoryReservationStatus.Active, reservation.Status);
    }

    private Task ApproveAsync(int salesOrderId)
        => fixture.ExecuteInScopeAsync(sp =>
            sp.GetRequiredService<ISalesOrderService>().ApproveAsync(salesOrderId));

    private Task PostAsync(int deliveryNoteId)
        => fixture.ExecuteInScopeAsync(sp =>
            sp.GetRequiredService<IDeliveryNoteService>().PostAsync(deliveryNoteId));

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
