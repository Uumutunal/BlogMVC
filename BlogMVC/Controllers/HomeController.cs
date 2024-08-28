using BlogMVC.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Json;
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
        public async Task<IActionResult> ListUnapprovedPosts()
        {
            ViewBag.Logged = HttpContext.Session.GetString("IsLogged");
            ViewBag.IsAdmin = HttpContext.Session.GetString("IsAdmin");

            List<PostViewModel> posts = new List<PostViewModel>();

            posts.Add(new PostViewModel() { Content = "Content", Title = "Title" });

            var allUnApprovedPosts = await _httpClient.GetFromJsonAsync<List<PostViewModel>>("https://localhost:7230/api/Post/AllUnApprovedPost");

            return View(allUnApprovedPosts);
        }


        public async Task<IActionResult> ApprovePost(Guid Id)
        {
            await _httpClient.PostAsJsonAsync("https://localhost:7230/api/Post/ApprovePost", Id);

            return RedirectToAction("ListUnapprovedPosts");
        }


        public async Task<IActionResult> ListRoles()
        {
            ViewBag.Logged = HttpContext.Session.GetString("IsLogged");
            ViewBag.IsAdmin = HttpContext.Session.GetString("IsAdmin");
            var roles = await _httpClient.GetFromJsonAsync<List<string>>("https://localhost:7230/api/Account/GetAllRoles");

            return View(roles);
        }

        [HttpPost]
        public async Task<IActionResult> AddRole(string role)
        {
            var roles = new List<string>() { role };
            var result = await _httpClient.PostAsJsonAsync("https://localhost:7230/api/Admin/Add-Role", roles);

            return RedirectToAction("ListRoles");
        }


        public async Task<IActionResult> AssignRole()
        {
            ViewBag.Logged = HttpContext.Session.GetString("IsLogged");
            ViewBag.IsAdmin = HttpContext.Session.GetString("IsAdmin");
            var roles = await _httpClient.GetFromJsonAsync<List<string>>("https://localhost:7230/api/Account/GetAllRoles");
            var users = await _httpClient.GetFromJsonAsync<List<UserViewModel>>("https://localhost:7230/api/Account/AllUser");



            var userList = new List<string>() {};

            foreach(var user in users)
            {
                userList.Add(user.Email);
            }

            ViewBag.ComboBox2Items = roles;
            ViewBag.ComboBox1Items = userList;

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AssignRole(string comboBox1, string comboBox2)
        {
            var updateRoleRequest = new UpdateRoleRequest { UserId = comboBox1, RoleName = comboBox2 };
            var result = await _httpClient.PutAsJsonAsync("https://localhost:7230/api/Admin/UpdateRole", updateRoleRequest);

            return RedirectToAction("AssignRole");
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
