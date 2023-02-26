using Microsoft.AspNetCore.Mvc;
using Play.Common.Repositories;
using Play.Inventory.Service.DTOs;
using Play.Inventory.Service.Entities;
using Play.Inventory.Service.Extensions;

namespace Play.Inventory.Service.Controllers;

[ApiController]
[Route("[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IRepository<InventoryItem> _itemsRepository;
    private readonly ILogger<ItemsController> _logger;

    public ItemsController(IRepository<InventoryItem> itemsRepository,
        ILogger<ItemsController> logger)
    {
        _itemsRepository = itemsRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItemDTO>>> GetAsync(Guid userId)
    {
        if (userId == Guid.Empty) return BadRequest();

        var items = await _itemsRepository.GetAllAsync(item => item.UserId == userId);
        var result = items.Select(item => item.AsDTO());
        
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> PostAsync(GrantItemsDTO grantItemsDTO)
    {
        var inventoryItem = await _itemsRepository.GetAsync(item => 
            item.UserId == grantItemsDTO.UserId && item.CatalogItemId == grantItemsDTO.CatalogItemId);

        if (inventoryItem is null)
        {
            inventoryItem = new InventoryItem
            {
                CatalogItemId = grantItemsDTO.CatalogItemId,
                UserId = grantItemsDTO.UserId,
                Quantity = grantItemsDTO.Quantity,
                AccuiredDate = DateTimeOffset.UtcNow
            };

            await _itemsRepository.CreateAsync(inventoryItem);
        }
        else
        {
            inventoryItem.Quantity += grantItemsDTO.Quantity;

            await _itemsRepository.UpdateAsync(inventoryItem);
        }
            
        return Ok();
    }
}