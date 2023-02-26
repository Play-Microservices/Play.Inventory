using Play.Inventory.Service.DTOs;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Extensions;

public static class MapperExtensions
{
    public static InventoryItemDTO AsDTO(this InventoryItem item, string name, string? description)
    {
        return new InventoryItemDTO(item.CatalogItemId,  name, description, item.Quantity, item.AccuiredDate);
    }
}