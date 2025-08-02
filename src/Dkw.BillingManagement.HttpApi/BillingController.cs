using Dkw.BillingManagement.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Dkw.BillingManagement;

public abstract class BillingManagementController : AbpControllerBase
{
    protected BillingManagementController()
    {
        LocalizationResource = typeof(BillingManagementResource);
    }
}
