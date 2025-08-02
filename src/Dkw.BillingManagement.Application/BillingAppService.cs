using Dkw.BillingManagement.Localization;
using Volo.Abp.Application.Services;

namespace Dkw.BillingManagement;

public abstract class BillingManagementAppService : ApplicationService
{
    protected BillingManagementAppService()
    {
        LocalizationResource = typeof(BillingManagementResource);
        ObjectMapperContext = typeof(DkwBillingManagementApplicationModule);
    }
}
