namespace Dkw.Abp.Billing;

public interface IProvinceRepository
{
    Province GetProvince(String code);
    IEnumerable<Province> GetAll();
    IEnumerable<String> Codes();
}