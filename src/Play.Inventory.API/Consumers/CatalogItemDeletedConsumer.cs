using MassTransit;
using Play.Catalog.Contracts;
using Play.Common.Repositories;
using Play.Inventory.API.Entities;

namespace Play.Inventory.API.Consumers;

public class CatalogItemDeletedConsumer(
    IRepository<CatalogItem> repository
) : IConsumer<CatalogItemDeleted>
{
    private readonly IRepository<CatalogItem> _repository = repository;
    
    public async Task Consume(ConsumeContext<CatalogItemDeleted> context)
    {
        var message = context.Message;

        var item = await _repository.GetAsync(message.ItemId);
        if (item is not null)
        {
            return;
        }

        await _repository.DeleteAsync(item!.Id);
    }
}