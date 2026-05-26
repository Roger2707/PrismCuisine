namespace PrismCuisine.BuildingBlocks.Domain.Entities;

public abstract class Entity
{
    public Guid Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    protected Entity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    protected Entity(Guid id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    protected void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
