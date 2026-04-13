using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlazorMon.Infrastructure.Identity;

namespace BlazorMon.Web.Pages.Account;

public class LogoutModel(SignInManager<ApplicationUser> signInManager) : PageModel
{
    public async Task<IActionResult> OnPostAsync()
    {
        await signInManager.SignOutAsync();
        return RedirectToPage("/Account/Login");
    }

    // Handle GET (e.g., navigating directly to /account/logout) gracefully
    public IActionResult OnGet() => RedirectToPage("/Account/Login");
}
