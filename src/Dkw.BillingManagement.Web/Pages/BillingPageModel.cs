using Dkw.BillingManagement.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Dkw.BillingManagement.Web.Pages;

/* Inherit your PageModel classes from this class.
 */
public abstract class BillingManagementPageModel : AbpPageModel
{
    protected BillingManagementPageModel()
    {
        LocalizationResourceType = typeof(BillingManagementResource);
        ObjectMapperContext = typeof(DkwBillingManagementWebModule);
    }
}
