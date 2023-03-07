using Play.Common.Entites;

namespace Play.Inventory.Service.Entities;

public class CatalogItem : IEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = String.Empty;

    public string? Description { get; set; }
}