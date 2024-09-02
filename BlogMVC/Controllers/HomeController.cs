using BlogMVC.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Web.Helpers;
using System.Xml.Linq;

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
                    HttpContext.Session.SetString("Role", "Admin");
                    ViewBag.IsAdmin = HttpContext.Session.GetString("IsAdmin");
                }
                else if (roles.Contains("Editor"))
                {
                    HttpContext.Session.SetString("Role", "Editor");
                }
                else
                {
                    HttpContext.Session.SetString("IsAdmin", "false");
                    HttpContext.Session.SetString("Role", "User");
                }

            }
            else
            {
                HttpContext.Session.SetString("IsAdmin", "false");
            }

            var postCategories = await _httpClient.GetFromJsonAsync<List<PostCategoryViewModel>>("https://localhost:7230/api/Post/AllPostCategories");
            var allPosts = await _httpClient.GetFromJsonAsync<List<PostViewModel>>("https://localhost:7230/api/Post/AllPost");

            // Extract PostId values from postCategories
            var postCategoryIds = new HashSet<Guid>(postCategories.Select(pc => pc.PostId));


            // Filter allPosts based on postCategoryIds
            var posts = allPosts.Where(post => postCategoryIds.Contains(post.Id)).ToList();

            return View(posts);
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
        public async Task<IActionResult> DeleteRole(string selectedRole)
        {
            var roles = new List<string>() { selectedRole };


            if (!string.IsNullOrEmpty(selectedRole))
            {

                var result = await _httpClient.PostAsJsonAsync("https://localhost:7230/api/Admin/DeleteRole", roles);
            }
            return RedirectToAction("ListRoles");
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



            var userList = new List<string>() { };

            foreach (var user in users)
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

        public async Task<IActionResult> Post(Guid id)
        {

            var postCategories = await _httpClient.GetFromJsonAsync<List<PostCategoryViewModel>>("https://localhost:7230/api/Post/AllPostCategories");
            var users = await _httpClient.GetFromJsonAsync<List<UserViewModel>>("https://localhost:7230/api/Account/AllUser");
            var posts = await _httpClient.GetFromJsonAsync<List<PostViewModel>>("https://localhost:7230/api/Post/AllPost");
            var comments = await _httpClient.GetFromJsonAsync<List<CommentViewModel>>("https://localhost:7230/api/Post/GetAllComments");
            var categories = await _httpClient.GetFromJsonAsync<List<CategoryViewModel>>("https://localhost:7230/api/Post/GetAllCategories");

            //var p = new List<PostCommentViewModel>();

            //foreach (var item in postComments)
            //{
            //    var p2 = new PostCommentViewModel();
            //    p2.User = users.FirstOrDefault(s => s.Id == item.UserId);
            //    p2.UserId = item.UserId;
            //    p2.Post = posts.FirstOrDefault(s => s.Id == item.PostId);
            //    p2.PostId = item.PostId;
            //    p2.Comment = comments.FirstOrDefault(s => s.Id == item.CommentId);
            //    p2.CommentId = item.CommentId;

            //    p.Add(p2);
            //}

            var p = new List<PostCategoryViewModel>();

            foreach (var item in postCategories)
            {
                var p2 = new PostCategoryViewModel();
                p2.Post = posts.FirstOrDefault(s => s.Id == item.PostId);
                p2.PostId = item.PostId;
                p2.User = users.FirstOrDefault(s => s.Id == item.UserId);
                p2.UserId = item.UserId;
                p2.Category = categories.FirstOrDefault(s => s.Id == item.CategoryId);
                p2.CategoryId = item.CategoryId;

                p.Add(p2);
            }

            var post2 = p.FirstOrDefault(x => x.PostId == id);


            ViewBag.Comments = new List<string>() { "comment" };
            ViewBag.Categories = new List<string>() { post2.Category.Name == null ? "": post2.Category.Name };
            ViewBag.Tags = new List<string>() { "tag" };
            ViewBag.Author = post2.User.Firstname + " " + post2.User.Lastname;
            ViewBag.AuthorPicture = post2.User.Photo;
            ViewBag.Date = post2.Post.CreatedDate;
            ViewBag.Role = HttpContext.Session.GetString("Role");

            var post = p.FirstOrDefault(x => x.PostId == id).Post;

            return View(post);
        }

        [HttpGet]
        public async Task<IActionResult> AddPost(Guid Id)
        {
            var posts = await _httpClient.GetFromJsonAsync<List<PostViewModel>>("https://localhost:7230/api/Post/AllPost");
            var post = posts.FirstOrDefault(x => x.Id == Id);

            var allCategories = await _httpClient.GetFromJsonAsync<List<CategoryViewModel>>("https://localhost:7230/api/Post/GetAllCategories");

            ViewBag.Categories = allCategories;

            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> AddPost(PostViewModel post, List<Guid> categories = null)
        {
            var userId = HttpContext.Session.GetString("UserId");

            var allCategories = await _httpClient.GetFromJsonAsync<List<CategoryViewModel>>("https://localhost:7230/api/Post/GetAllCategories");

            ViewBag.Categories = allCategories;

            var postRequest = new AddPostRequest();
            postRequest.Post = post;
            postRequest.UserId = userId;
            postRequest.CategoryIds = categories;

            if(post.Id == Guid.Empty)
            {
                var result = await _httpClient.PostAsJsonAsync("https://localhost:7230/api/Post/AddPost", postRequest);
            }
            else
            {
                var result = await _httpClient.PostAsJsonAsync("https://localhost:7230/api/Post/UpdatePost", postRequest);
            }



            return RedirectToAction("Index");
        }


        //[HttpGet]
        //public async Task<IActionResult> AddPost(Guid Id)
        //{
        //    var posts = await _httpClient.GetFromJsonAsync<List<PostViewModel>>("https://localhost:7230/api/Post/AllPost");
        //    var post = posts.FirstOrDefault(x => x.Id == Id);
        //    return View(post);
        //}

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
