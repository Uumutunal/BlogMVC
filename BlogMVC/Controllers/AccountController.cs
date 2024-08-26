using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Twitter;
using System.Threading.Tasks;
using BlogMVC.Models;
using System.Net.Http;

namespace BlogMVC.Controllers
{
    public class AccountController : Controller
    {
        private const string XsrfKey = "XsrfId";
        private readonly HttpClient _httpClient;

        public AccountController(HttpClient httpClient)
        {
            _httpClient = httpClient;

        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if(username == "admin" && password == "123")
            {
                ViewBag.Login = "true";
                HttpContext.Session.SetString("IsLogged", "true");
                ViewBag.IsAdmin = "true";
                HttpContext.Session.SetString("IsAdmin", "true");

                return RedirectToAction("Index", "Home");
            }
            ViewBag.Login = "false";

            return View();
        }

        public async Task<IActionResult> Register()
        {

            //var users = await _httpClient.GetFromJsonAsync<List<UserViewModel>>("https://localhost:5042/api/Account/users");

            return View();
		}
        [HttpPost]
        public IActionResult Register(string username, string email, string password, string confirmPassword)
        {
            return View();
        }

		public IActionResult Profile()
		{
            var user = new UserViewModel() { Email = "a@gmail.com", Username = "admin", Password = "123" };
            ViewBag.Logged = HttpContext.Session.GetString("IsLogged");
            ViewBag.IsAdmin = HttpContext.Session.GetString("IsAdmin");
            return View(user);
		}

        [HttpPost]
        public IActionResult Profile(string username, string email, string password)
        {
            var user = new UserViewModel() { Email = email, Username = username, Password = password };
            return View(user);
        }

		public IActionResult Logout()
		{
            ViewBag.Logout = "false";
			HttpContext.Session.SetString("IsLogged", "false");
			HttpContext.Session.SetString("IsAdmin", "false");

			return RedirectToAction("Index", "Home");
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

			HttpContext.Session.SetString("IsLogged", "true");
			return RedirectToAction("Index", "Home");
        }
    }
}
