namespace Dkw.Abp.Billing;

public interface IItemRepository
{
    void AddItem(Item item);
    void RemoveItem(Guid id);
    IEnumerable<Item> GetAllItems();
    Item GetItemById(Guid id);
}
