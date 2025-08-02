namespace Dkw.Abp.Billing;

public interface ITaxCodeRepository
{
    void Add(TaxCode taxCode);
    void Delete(string code);
    IEnumerable<TaxCode> GetAll();
    TaxCode? GetByCode(string code);
    void Update(TaxCode taxCode);
}