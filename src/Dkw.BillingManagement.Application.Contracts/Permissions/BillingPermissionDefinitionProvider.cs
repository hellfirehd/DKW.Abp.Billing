using Dkw.BillingManagement.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Dkw.BillingManagement.Permissions;

public class BillingManagementPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(BillingManagementPermissions.GroupName, L("Permission:BillingManagement"));
    }

    private static LocalizableString L(String name)
    {
        return LocalizableString.Create<BillingManagementResource>(name);
    }
}
