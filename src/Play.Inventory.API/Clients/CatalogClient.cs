using System.Net.Http.Json;
using Play.Inventory.API.DTOs;

namespace Play.Inventory.API.Clients;

public class CatalogClient
{
    private readonly HttpClient _httpClient;

    public CatalogClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<CatalogItemDTO>> GetCatalogItemsAsync()
    {
        var items = await _httpClient.GetFromJsonAsync<IReadOnlyCollection<CatalogItemDTO>>("/items");

        return items!;
    }
}