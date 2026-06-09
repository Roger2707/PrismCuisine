namespace PrismERP.BuildingBlocks.Domain.Entities;

public abstract class Entity
{
    public int Id { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    protected Entity()
    {
        CreatedAt = DateTime.UtcNow;
    }

    protected void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    protected void MarkDeleted()
    {
        IsDeleted = true;
        Touch();
    }
}
