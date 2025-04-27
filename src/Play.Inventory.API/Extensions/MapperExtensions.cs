using Play.Inventory.API.DTOs;
using Play.Inventory.API.Entities;

namespace Play.Inventory.API.Extensions;

public static class MapperExtensions
{
    public static InventoryItemDTO AsDTO(this InventoryItem item, string name, string? description)
    {
        return new InventoryItemDTO(item.CatalogItemId,  name, description, item.Quantity, item.AccuiredDate);
    }
}