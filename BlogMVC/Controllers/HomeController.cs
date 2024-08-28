using BlogMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;

namespace BlogMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;

        public HomeController(ILogger<HomeController> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;

        }


        public async Task<IActionResult> Index()
        {

            if (HttpContext.Session.GetString("IsLogged") != null)
            {
                ViewBag.Logged = HttpContext.Session.GetString("IsLogged");
                ViewBag.IsAdmin = HttpContext.Session.GetString("IsAdmin");
            }
            else
            {
                ViewBag.Logged = "false";
            }

            if (HttpContext.Session.GetString("Token") != null)
            {
                var token = HttpContext.Session.GetString("Token");


                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                // Extract roles from the claims
                var roles = jwtToken.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

                if (roles.Contains("Admin"))
                {
                    HttpContext.Session.SetString("IsAdmin", "true");
                    ViewBag.IsAdmin = HttpContext.Session.GetString("IsAdmin");
                }
                else
                {
                    HttpContext.Session.SetString("IsAdmin", "false");
                }
            }
            else
            {
                HttpContext.Session.SetString("IsAdmin", "false");
            }

            var allPosts = await _httpClient.GetFromJsonAsync<List<PostViewModel>>("https://localhost:7230/api/Post/AllPost");
            var allCategories = await _httpClient.GetFromJsonAsync<List<PostCategoryViewModel>>("https://localhost:7230/api/Post/AllPostCategories");

            return View(allPosts);
        }

        [Authorize(Policy = "AdminOnly")]
        public IActionResult ListUnapprovedPosts()
        {
            ViewBag.Logged = HttpContext.Session.GetString("IsLogged");
            ViewBag.IsAdmin = HttpContext.Session.GetString("IsAdmin");

            List<PostViewModel> posts = new List<PostViewModel>();

            posts.Add(new PostViewModel() { Content = "Content", Title = "Title" });

            return View(posts);
        }


        public IActionResult ApprovePost(Guid Id)
        {
            return RedirectToAction("ListUnapprovedPosts");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
