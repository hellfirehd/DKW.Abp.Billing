using Dkw.BillingManagement.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Dkw.BillingManagement.Pages;

public abstract class BillingManagementPageModel : AbpPageModel
{
    protected BillingManagementPageModel()
    {
        LocalizationResourceType = typeof(BillingManagementResource);
    }
}
