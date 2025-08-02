using Microsoft.AspNetCore.Authentication;

namespace Dkw.BillingManagement.Pages;

public class IndexModel : BillingManagementPageModel
{
    public void OnGet()
    {

    }

    public async Task OnPostLoginAsync()
    {
        await HttpContext.ChallengeAsync("oidc");
    }
}
