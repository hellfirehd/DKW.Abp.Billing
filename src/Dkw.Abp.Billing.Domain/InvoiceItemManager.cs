namespace Dkw.Abp.Billing;

public class InvoiceItemManager(IItemRepository itemRepository, IEnumerable<IInvoiceItemFactory> itemFactories)
{
    private IItemRepository ItemRepository { get; } = itemRepository;
    private IEnumerable<IInvoiceItemFactory> ItemFactories { get; } = itemFactories;

    public InvoiceItem Create(Guid itemId, DateOnly effectiveDate, Decimal quantity)
    {
        var product = ItemRepository.GetItemById(itemId);
        if (product == null)
        {
            throw new ArgumentException("Invalid item ID.");
        }

        var factory = ItemFactories.FirstOrDefault(f => f.CanCreate(product));
        if (factory == null)
        {
            throw new InvalidOperationException("No factory available to create the invoice item.");
        }

        var item = factory.Create(product, effectiveDate);

        item.ChangeQuantity(quantity);

        return item;
    }
}