using MassTransit;
using Play.Common.Repositories;
using Play.Inventory.API.Entities;
using Play.Inventory.API.Exceptions;
using Play.Inventory.Contracts;

namespace Play.Inventory.API.Consumers;

public class GrantItemsConsumer(
    IRepository<InventoryItem> inventoryItemsRepository,
    IRepository<CatalogItem> catalogItemsRepository) : IConsumer<GrantItems>
{
    private readonly IRepository<InventoryItem> _inventoryItemsRepository = inventoryItemsRepository;
    private readonly IRepository<CatalogItem> _catalogItemsRepository = catalogItemsRepository;
    
    public async Task Consume(ConsumeContext<GrantItems> context)
    {
        var message = context.Message;
        var catalogItem = await _catalogItemsRepository.GetAsync(message.CatalogItemId);
        if (catalogItem is null)
        {
            throw new UnknownItemException(message.CatalogItemId);
        }
        
        var inventoryItem = await _inventoryItemsRepository.GetAsync(item => 
            item.UserId == message.UserId && item.CatalogItemId == message.CatalogItemId);

        if (inventoryItem == null)
        {
            inventoryItem = new InventoryItem
            {
                CatalogItemId = message.CatalogItemId,
                UserId = message.UserId,
                Quantity = message.Quantity,
                AccuiredDate = DateTimeOffset.UtcNow
            };
            
            inventoryItem.MessageIds.Add(context.MessageId!.Value);

            await _inventoryItemsRepository.CreateAsync(inventoryItem);
        }
        else if (!inventoryItem.MessageIds.Contains(context.MessageId!.Value))
        {
            inventoryItem.Quantity += message.Quantity;

            await _inventoryItemsRepository.UpdateAsync(inventoryItem);
        }
        
        var itemsGrantedTask = context.Publish(new InventoryItemsGranted(message.CorrelationId));
        var invetoryUpdatedTask = context.Publish(new InventoryItemUpdated(
            inventoryItem.UserId, 
            inventoryItem.CatalogItemId, 
            inventoryItem.Quantity));
        await Task.WhenAll(itemsGrantedTask, invetoryUpdatedTask);
    }
}