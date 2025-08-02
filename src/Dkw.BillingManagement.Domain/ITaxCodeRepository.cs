namespace Dkw.BillingManagement;

public interface ITaxCodeRepository
{
    void Add(TaxCode taxCode);
    void Delete(String code);
    IEnumerable<TaxCode> GetAll();
    TaxCode? GetByCode(String code);
    void Update(TaxCode taxCode);
}