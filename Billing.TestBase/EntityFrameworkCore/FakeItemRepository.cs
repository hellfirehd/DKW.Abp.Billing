namespace Billing.EntityFrameworkCore;

public class FakeItemRepository : IItemRepository
{
    private readonly List<Item> _items = [];

    public void AddItem(Item item)
    {
        _items.Add(item);
    }

    public IEnumerable<Item> GetAllItems()
    {
        return _items;
    }

    public Item GetItemById(Guid id)
    {
        return _items.Single(i => i.Id == id);
    }

    public void RemoveItem(Guid id)
    {
        _items.RemoveAll(i => i.Id == id);
    }
}
