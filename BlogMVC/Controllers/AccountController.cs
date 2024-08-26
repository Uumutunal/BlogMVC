using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Twitter;
using System.Threading.Tasks;

namespace BlogMVC.Controllers
{
    public class AccountController : Controller
    {
        private const string XsrfKey = "XsrfId";

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            return View();
        }

        public IActionResult Register()
		{
			return View();
		}
		public IActionResult Index()
        {
            return View();
        }


        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

            return Challenge(properties, provider);
        }

        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null)
        {
            var loginInfo = await HttpContext.AuthenticateAsync();

            if (loginInfo?.Principal == null)
            {
                // Handle login failure
                return RedirectToAction("Login");
            }

            // Handle login success, e.g., sign in the user or create a new account
            return RedirectToLocal(returnUrl);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
