namespace Dkw.BillingManagement;

public interface IProvinceRepository
{
    Province GetProvince(String code);
    IEnumerable<Province> GetAll();
    IEnumerable<String> Codes();
}