namespace Play.Inventory.Service.DTOs;

public record GrantItemsDTO(Guid UserId, Guid CatalogItemId, int Quantity);

public record InventoryItemDTO(Guid CalagotItemId, int Quantity, DateTimeOffset AccuiredDate);