using Microsoft.AspNetCore.Mvc;
using Play.Common.Repositories;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.DTOs;
using Play.Inventory.Service.Entities;
using Play.Inventory.Service.Extensions;

namespace Play.Inventory.Service.Controllers;

[ApiController]
[Route("[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IRepository<InventoryItem> _itemsRepository;
    private readonly CatalogClient _catalogClient;
    private readonly ILogger<ItemsController> _logger;

    public ItemsController(IRepository<InventoryItem> itemsRepository,
        CatalogClient catalogClient,
        ILogger<ItemsController> logger)
    {
        _itemsRepository = itemsRepository;
        _catalogClient = catalogClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItemDTO>>> GetAsync(Guid userId)
    {
        if (userId == Guid.Empty) return BadRequest();
        var catalogItems = await _catalogClient.GetCatalogItemsAsync();
        var inventoryItems = await _itemsRepository.GetAllAsync(item => item.UserId == userId);
        var inventoryItemDTOs = inventoryItems.Select(inventoryItem => 
        {
            var catalogItem = catalogItems.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
            return inventoryItem.AsDTO(catalogItem.Name, catalogItem.Description);
        });
        
        return Ok(inventoryItemDTOs);
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