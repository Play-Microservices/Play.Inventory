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
        var item = await _catalogItemsRepository.GetAsync(message.CatalogItemId);
        if (item is null)
        {
            throw new UnknownItemException(message.CatalogItemId);
        }
        
        var inventoryItem = await _inventoryItemsRepository.GetAsync(item => 
            item.UserId == message.UserId && item.CatalogItemId == message.CatalogItemId);

        if (inventoryItem is null)
        {
            inventoryItem = new InventoryItem
            {
                CatalogItemId = message.CatalogItemId,
                UserId = message.UserId,
                Quantity = message.Quantity,
                AccuiredDate = DateTimeOffset.UtcNow
            };

            await _inventoryItemsRepository.CreateAsync(inventoryItem);
        }
        else
        {
            inventoryItem.Quantity += message.Quantity;

            await _inventoryItemsRepository.UpdateAsync(inventoryItem);
        }
        
        await context.Publish(new InventoryItemsGranted(message.CorrelationId));
    }
}