using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Twitter;
using System.Threading.Tasks;
using BlogMVC.Models;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Generic;


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
        public async Task<IActionResult> Login(string email, string password)
        {

            var user = new UserViewModel()
            {
                Email = email,
                Password = password,
                Username = ""
            };

            var login = await _httpClient.PostAsJsonAsync("https://localhost:7230/api/Account/Login", user);



            if (login.IsSuccessStatusCode)
            {
                var loginResponse = await login.Content.ReadFromJsonAsync<LoginResponse>();

                if (loginResponse != null)
                {
                    // Store token and user ID in session or cookies
                    HttpContext.Session.SetString("IsLogged", "true");
                    HttpContext.Session.SetString("UserId", loginResponse.UserId);
                    HttpContext.Session.SetString("Token", loginResponse.Token);

                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(loginResponse.Token);

                    // Extract roles from the claims
                    var roles = jwtToken.Claims
                        .Where(c => c.Type == ClaimTypes.Role)
                        .Select(c => c.Value)
                        .ToList();


                    var claims = new List<Claim>
                    {
                        //new Claim(ClaimTypes.Role, "Admin")
                    };

                    if (roles.Any())
                    {
                        foreach (var role in roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));
                        }
                    }
                    else
                    {
                        claims.Add(new Claim(ClaimTypes.Role, "NoRole"));
                    }
                    claims.Add(new Claim(ClaimTypes.Name, email));


                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        new AuthenticationProperties
                        {
                            IsPersistent = true
                        });

                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Login = "false";

            return View();
        }

        public async Task<IActionResult> Register()
        {


            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword)
        {
            var user = new UserViewModel()
            {
                Username = username,
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword
            };

            var register = await _httpClient.PostAsJsonAsync("https://localhost:7230/api/Account/Register", user);

            if (register.IsSuccessStatusCode)
            {
                ViewBag.Login = "true";
                HttpContext.Session.SetString("IsLogged", "true");
                return RedirectToAction("Index", "Home");

            }

            return View();
        }

        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Profile()
        {
            ViewBag.Login = "true";
            HttpContext.Session.SetString("IsLogged", "true");

            var token = HttpContext.Session.GetString("Token");
            var loggedUserId = HttpContext.Session.GetString("UserId");


            var loggedUser = await _httpClient.GetAsync($"https://localhost:7230/api/Account/user?id={loggedUserId}");

            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (loggedUser.IsSuccessStatusCode)
            {
                var userLogged = await loggedUser.Content.ReadFromJsonAsync<UserViewModel>();
                ViewBag.Logged = HttpContext.Session.GetString("IsLogged");
                ViewBag.IsAdmin = HttpContext.Session.GetString("IsAdmin");
                return View(userLogged);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Profile(UserViewModel user)
        {
            ViewBag.Login = "true";
            HttpContext.Session.SetString("IsLogged", "true");

            var userUpdate = new UserViewModel() { Email = user.Email, Username = user.Username, Password = user.Password, Firstname = user.Firstname, Lastname = user.Lastname };

            //user.ConfirmPassword = "";
            if (user.ConfirmPassword == null)
            {
                user.ConfirmPassword = "";
            }
            user.Id = HttpContext.Session.GetString("UserId");

            var result = await _httpClient.PutAsJsonAsync("https://localhost:7230/api/Account/Update", user);



            return View(userUpdate);
        }

        public IActionResult Logout()
        {
            ViewBag.Logout = "false";
            //HttpContext.Session.SetString("IsLogged", "false");
            //HttpContext.Session.SetString("IsAdmin", "false");
            //HttpContext.Session.SetString("UserId", "");

            HttpContext.Session.Clear();


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

            var claims = loginInfo.Principal.Claims;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

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
