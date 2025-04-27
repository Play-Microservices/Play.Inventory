using MassTransit;
using Play.Catalog.Contracts;
using Play.Common.Repositories;
using Play.Inventory.API.Entities;

namespace Play.Inventory.API.Consumers;

public class CatalogItemCreatedConsumer(
    IRepository<CatalogItem> repository
) : IConsumer<CatalogItemCreated>
{
    private readonly IRepository<CatalogItem> _repository = repository;
    
    public async Task Consume(ConsumeContext<CatalogItemCreated> context)
    {
        var message = context.Message;

        var item = await _repository.GetAsync(message.ItemId);
        if (item is not null)
        {
            return;
        }

        item = new CatalogItem
        {
            Id = message.ItemId,
            Name = message.Name,
            Description = message.Description
        };
        await _repository.CreateAsync(item);
    }
}