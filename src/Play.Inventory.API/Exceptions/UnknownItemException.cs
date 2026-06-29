namespace Play.Inventory.API.Exceptions;

[Serializable]
public class UnknownItemException : Exception
{
    public UnknownItemException(Guid itemId) : base($"Unknown ItemId: '{itemId}'")
    {
        ItemId = itemId;
    }

    public Guid ItemId { get; }
}