using MassTransit;
using Play.Common.Repositories;
using Play.Inventory.API.Entities;
using Play.Inventory.API.Exceptions;
using Play.Inventory.Contracts;

namespace Play.Inventory.API.Consumers;

public class SubtractItemsConsumer(
    IRepository<InventoryItem> inventoryItemsRepository,
    IRepository<CatalogItem> catalogItemsRepository) : IConsumer<SubtractItems>
{
    private readonly IRepository<InventoryItem> _inventoryItemsRepository = inventoryItemsRepository;
    private readonly IRepository<CatalogItem> _catalogItemsRepository = catalogItemsRepository;
    
    public async Task Consume(ConsumeContext<SubtractItems> context)
    {
        var message = context.Message;
        var item = await _catalogItemsRepository.GetAsync(message.CatalogItemId);
        if (item is null)
        {
            throw new UnknownItemException(message.CatalogItemId);
        }

        var inventoryItem = await _inventoryItemsRepository.GetAsync(item => 
            item.UserId == message.UserId && item.CatalogItemId == message.CatalogItemId);

        if (inventoryItem != null && !inventoryItem.MessageIds.Contains(context.MessageId!.Value))
        {
            inventoryItem.Quantity -= message.Quantity;

            await _inventoryItemsRepository.UpdateAsync(inventoryItem);
            await context.Publish(new InventoryItemUpdated(
                inventoryItem.UserId, 
                inventoryItem.CatalogItemId, 
                inventoryItem.Quantity));
            
            inventoryItem.MessageIds.Add(context.MessageId!.Value);
        }
        
        await context.Publish(new InventoryItemsSubtracted(message.CorrelationId));
    }
}