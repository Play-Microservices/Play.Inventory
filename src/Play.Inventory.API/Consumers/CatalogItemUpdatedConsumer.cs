using MassTransit;
using Play.Catalog.Contracts;
using Play.Common.Repositories;
using Play.Inventory.API.Entities;

namespace Play.Inventory.API.Consumers;

public class CatalogItemUpdatedConsumer(
    IRepository<CatalogItem> repository
) : IConsumer<CatalogItemUpdated>
{
    private readonly IRepository<CatalogItem> _repository = repository;

    public async Task Consume(ConsumeContext<CatalogItemUpdated> context)
    {
        var message = context.Message;

        var item = await _repository.GetAsync(message.ItemId);
        if (item is null)
        {
            item = new CatalogItem
            {
                Id = message.ItemId,
                Name = message.Name,
                Description = message.Description
            };

            await _repository.CreateAsync(item);
        }
        else
        {
            item.Name = message.Name;
            item.Description = message.Description;
            
            await _repository.UpdateAsync(item);
        }
    }
}