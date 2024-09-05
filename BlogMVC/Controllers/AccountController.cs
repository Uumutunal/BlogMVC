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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNet.Identity;


namespace BlogMVC.Controllers
{
    public class AccountController : Controller
    {
        private const string XsrfKey = "XsrfId";
        private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountController(HttpClient httpClient, IWebHostEnvironment webHostEnvironment)
        {
            _httpClient = httpClient;
            _webHostEnvironment = webHostEnvironment;
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
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword, IFormFile photo, string firstname, string lastname)
        {
            string photoPath = "";
            if (photo != null)
            {
                string uniqueFileName = "";

                // Define the path to save the image
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");

                // Generate a unique file name to avoid conflicts
                uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(photo.FileName);

                // Combine the path with the file name
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save the file to the specified path
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(fileStream);
                }

                // Set the PhotoPath property to the relative path
                photoPath = "/images/" + uniqueFileName;
            }

            var user = new UserViewModel()
            {
                Username = username,
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword,
                Photo = photoPath,
                Firstname = firstname,
                Lastname = lastname
            };

            var register = await _httpClient.PostAsJsonAsync("https://localhost:7230/api/Account/Register", user);

            if (register.IsSuccessStatusCode)
            {
                var registeredUser = await register.Content.ReadFromJsonAsync<UserViewModel>();

                ViewBag.Login = "true";
                HttpContext.Session.SetString("IsLogged", "true");
                HttpContext.Session.SetString("UserId", registeredUser.Id);
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
        public async Task<IActionResult> Profile(UserViewModel user, IFormFile image)
        {
            ViewBag.Logged = HttpContext.Session.GetString("IsLogged");
            ViewBag.IsAdmin = HttpContext.Session.GetString("IsAdmin");

            ViewBag.Login = "true";
            HttpContext.Session.SetString("IsLogged", "true");

            string photoPath = "";
            if (image != null)
            {
                string uniqueFileName = "";

                // Define the path to save the image
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");

                // Generate a unique file name to avoid conflicts
                uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);

                // Combine the path with the file name
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save the file to the specified path
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                // Set the PhotoPath property to the relative path
                photoPath = "/images/" + uniqueFileName;
            }


            //user.ConfirmPassword = "";
            if (user.ConfirmPassword == null)
            {
                user.ConfirmPassword = "";
            }
            user.Id = HttpContext.Session.GetString("UserId");

            var userUpdate = new UserViewModel() { Email = user.Email, Username = user.Username, Password = user.Password, Firstname = user.Firstname, Lastname = user.Lastname, Photo = photoPath, Id = user.Id, ConfirmPassword = user.ConfirmPassword };


            var result = await _httpClient.PutAsJsonAsync("https://localhost:7230/api/Account/Update", userUpdate);



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

        public async Task<IActionResult> DeleteAccount()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var user = await _httpClient.GetAsync($"https://localhost:7230/api/Account/user?id={userId}");
            var loggedUser = await user.Content.ReadFromJsonAsync<UserViewModel>();
            
            string imageFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
            string fileName = Path.GetFileName(loggedUser.Photo);
            string filePath = Path.Combine(imageFolderPath, fileName);


            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            var result = await _httpClient.PostAsync($"https://localhost:7230/api/Account/DeleteAccount?id={userId}", null);


            return RedirectToAction("Logout");
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
