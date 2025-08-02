using Dkw.BillingManagement.Localization;
using Volo.Abp.AspNetCore.Components;

namespace Dkw.BillingManagement.Blazor;

public abstract class BillingManagementComponentBase : AbpComponentBase
{
    protected BillingManagementComponentBase()
    {
        LocalizationResource = typeof(BillingManagementResource);
    }
}
