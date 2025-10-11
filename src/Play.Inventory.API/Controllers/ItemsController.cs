using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Play.Common.Repositories;
using Play.Inventory.API.DTOs;
using Play.Inventory.API.Entities;
using Play.Inventory.API.Extensions;

namespace Play.Inventory.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ItemsController(
    IRepository<InventoryItem> inventoryItemsRepository,
    IRepository<CatalogItem> catalogItemsRepository,
    ILogger<ItemsController> logger) : ControllerBase
{
    private const string AdminRole = "Admin";

    private readonly IRepository<InventoryItem> _inventoryItemsRepository = inventoryItemsRepository;
    private readonly IRepository<CatalogItem> _catalogItemsRepository = catalogItemsRepository;
    private readonly ILogger<ItemsController> _logger = logger;

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<InventoryItemDTO>>> GetAsync(Guid userId)
    {
        if (userId == Guid.Empty) return BadRequest();

        var user = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (user == null || (Guid.Parse(user) != userId && !User.IsInRole(AdminRole)))
        {
            return Forbid();
        }
        
        var inventoryItemEntities = await _inventoryItemsRepository.GetAllAsync(item => item.UserId == userId);
        var itemIds = inventoryItemEntities.Select(item => item.CatalogItemId);
        var catalogItemEntities = await _catalogItemsRepository.GetAllAsync(item => itemIds.Contains(item.Id));
        if (catalogItemEntities.Count != itemIds.Count())
        {
            _logger.LogWarning("Some catalog items are missing for user {UserId}", userId);
            return NotFound();
        }

        var inventoryItemDTOs = inventoryItemEntities.Select(inventoryItem => 
        {
            var catalogItem = catalogItemEntities.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
            return inventoryItem.AsDTO(catalogItem.Name, catalogItem.Description);
        });
        
        return Ok(inventoryItemDTOs);
    }

    [HttpPost]
    [Authorize(Roles = AdminRole)]
    public async Task<ActionResult> PostAsync(GrantItemsDTO grantItemsDTO)
    {
        var inventoryItem = await _inventoryItemsRepository.GetAsync(item => 
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

            await _inventoryItemsRepository.CreateAsync(inventoryItem);
        }
        else
        {
            inventoryItem.Quantity += grantItemsDTO.Quantity;

            await _inventoryItemsRepository.UpdateAsync(inventoryItem);
        }
            
        return Ok();
    }
}