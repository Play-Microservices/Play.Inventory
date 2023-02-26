namespace Play.Inventory.Service.DTOs;

public record GrantItemsDTO(Guid UserId, Guid CatalogItemId, int Quantity);

public record InventoryItemDTO(Guid CalagotItemId, string Name, string? Description, int Quantity, DateTimeOffset AccuiredDate);