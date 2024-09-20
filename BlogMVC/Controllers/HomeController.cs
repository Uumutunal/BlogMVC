using BlogMVC.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, HttpClient httpClient, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _httpClient = httpClient;
            _webHostEnvironment = webHostEnvironment;
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
                    ViewBag.Role = HttpContext.Session.GetString("Role");
                }
                else if (roles.Contains("Editor"))
                {
                    HttpContext.Session.SetString("Role", "Editor");
                    HttpContext.Session.SetString("IsAdmin", "true");
                    ViewBag.IsAdmin = HttpContext.Session.GetString("IsAdmin");
                    ViewBag.Role = HttpContext.Session.GetString("Role");
                }
                else if (roles.Contains("Author"))
                {
                    HttpContext.Session.SetString("Role", "Author");
                    HttpContext.Session.SetString("IsAdmin", "false");
                    ViewBag.IsAdmin = HttpContext.Session.GetString("IsAdmin");
                    ViewBag.Role = HttpContext.Session.GetString("Role");
                }
                else if (roles.Contains("Subscriber"))
                {
                    HttpContext.Session.SetString("Role", "Subscriber");
                    HttpContext.Session.SetString("IsAdmin", "false");
                    ViewBag.IsAdmin = HttpContext.Session.GetString("IsAdmin");
                    ViewBag.Role = HttpContext.Session.GetString("Role");
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

            var postCategories = await _httpClient.GetFromJsonAsync<List<PostCategoryViewModel>>("https://blogy1.azurewebsites.net/api/Post/AllPostCategories");
            var allPosts = await _httpClient.GetFromJsonAsync<List<PostViewModel>>("https://blogy1.azurewebsites.net/api/Post/AllPost");
            var allUsers = await _httpClient.GetFromJsonAsync<List<UserViewModel>>("https://blogy1.azurewebsites.net/api/Account/AllUser");
            var allCategories = await _httpClient.GetFromJsonAsync<List<CategoryViewModel>>("https://blogy1.azurewebsites.net/api/Post/GetAllCategories");

            var posts = new List<PostCategoryViewModel>();

            foreach (var item in postCategories)
            {
                var newPostCategory = new PostCategoryViewModel();

                var user = allUsers.FirstOrDefault(x => x.Id == item.UserId);
                item.User = user;
                newPostCategory.User = user;

                var category = allCategories.FirstOrDefault(x => x.Id == item.CategoryId);
                item.Category = category;
                newPostCategory.Category = category;

                var post = allPosts.FirstOrDefault(x => x.Id == item.PostId);
                if (post == null)
                {
                    continue;
                }
                else
                {
                    item.Post = post;
                    newPostCategory.Post = post;
                    newPostCategory.PostId = post.Id;
                    posts.Add(newPostCategory);
                }


            }


            var categories = new List<CategoryViewModel>();

            foreach (var category in allCategories)
            {
                foreach (var item in posts)
                {
                    if (item.Category.Name == category.Name)
                    {
                        if (!categories.Contains(category))
                        {
                            categories.Add(category);
                        }
                    }
                }
            }


            ViewBag.Categories = categories;

            return View(posts);
        }

        public async Task<IActionResult> ViewCategoryDetail(Guid categoryId)
        {
            ViewBag.Logged = HttpContext.Session.GetString("IsLogged");
            ViewBag.Role = HttpContext.Session.GetString("Role");

            var postCategories = await _httpClient.GetFromJsonAsync<List<PostCategoryViewModel>>("https://blogy1.azurewebsites.net/api/Post/AllPostCategories");

            var allPosts = await _httpClient.GetFromJsonAsync<List<PostViewModel>>("https://blogy1.azurewebsites.net/api/Post/AllPost");
            var allUsers = await _httpClient.GetFromJsonAsync<List<UserViewModel>>("https://blogy1.azurewebsites.net/api/Account/AllUser");
            var allCategories = await _httpClient.GetFromJsonAsync<List<CategoryViewModel>>("https://blogy1.azurewebsites.net/api/Post/GetAllCategories");

            foreach (var item in postCategories)
            {
                var user = allUsers.FirstOrDefault(x => x.Id == item.UserId);
                item.User = user;

                var category = allCategories.FirstOrDefault(x => x.Id == item.CategoryId);
                item.Category = category;

                var post = allPosts.FirstOrDefault(x => x.Id == item.PostId);
                if (post == null)
                {
                    continue;
                }
                item.Post = post;

            }

            return View(postCategories.Where(x => x.CategoryId == categoryId).ToList());
        }

        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ListUnapprovedPosts()
        {
            ViewBag.Logged = HttpContext.Session.GetString("IsLogged");
            ViewBag.Role = HttpContext.Session.GetString("Role");



            var postCategories = await _httpClient.GetFromJsonAsync<List<PostCategoryViewModel>>("https://blogy1.azurewebsites.net/api/Post/AllPostCategories");
            var allPosts = await _httpClient.GetFromJsonAsync<List<PostViewModel>>("https://blogy1.azurewebsites.net/api/Post/AllUnApprovedPost");
            var allUsers = await _httpClient.GetFromJsonAsync<List<UserViewModel>>("https://blogy1.azurewebsites.net/api/Account/AllUser");
            var allCategories = await _httpClient.GetFromJsonAsync<List<CategoryViewModel>>("https://blogy1.azurewebsites.net/api/Post/GetAllCategories");


            var posts = new List<PostCategoryViewModel>();


            foreach (var item in postCategories)
            {
                var newPostCategory = new PostCategoryViewModel();

                var user = allUsers.FirstOrDefault(x => x.Id == item.UserId);
                item.User = user;
                newPostCategory.User = user;

                var category = allCategories.FirstOrDefault(x => x.Id == item.CategoryId);
                item.Category = category;
                newPostCategory.Category = category;

                var post = allPosts.FirstOrDefault(x => x.Id == item.PostId);
                if (post == null)
                {
                    continue;
                }
                else
                {
                    item.Post = post;
                    newPostCategory.Post = post;
                    newPostCategory.PostId = post.Id;
                    posts.Add(newPostCategory);
                }


            }


            var categories = new List<CategoryViewModel>();

            foreach (var category in allCategories)
            {
                foreach (var item in posts)
                {
                    if (item.Category.Name == category.Name)
                    {
                        if (!categories.Contains(category))
                        {
                            categories.Add(category);
                        }
                    }
                }
            }


            ViewBag.Categories = categories;

            var allUnApprovedPosts = await _httpClient.GetFromJsonAsync<List<PostViewModel>>("https://blogy1.azurewebsites.net/api/Post/AllUnApprovedPost");

            return View(posts);
        }
        public async Task<IActionResult> RemoveFromFavorites(Guid postId)
        {
            string userId = HttpContext.Session.GetString("UserId");

            var allFavoritePosts = await _httpClient.GetFromJsonAsync<List<FavoritePostViewModel>>("https://blogy1.azurewebsites.net/api/Post/GetAllUserFavorites");

            var postToDelete = allFavoritePosts.FirstOrDefault(x => x.PostId == postId && x.UserId == userId);
            

            var result = await _httpClient.PostAsJsonAsync("https://blogy1.azurewebsites.net/api/Post/RemoveFromFavorites", postToDelete.Id);

            return RedirectToAction("GetAllFavoritePosts");
        }


        public async Task<IActionResult> GetAllFavoritePosts()
        {
            ViewBag.Logged = HttpContext.Session.GetString("IsLogged");
            ViewBag.Role = HttpContext.Session.GetString("Role");

            string userId = HttpContext.Session.GetString("UserId");

            //
            var postCategories = await _httpClient.GetFromJsonAsync<List<PostCategoryViewModel>>("https://blogy1.azurewebsites.net/api/Post/AllPostCategories");
            var allFavoritePosts = await _httpClient.GetFromJsonAsync<List<FavoritePostViewModel>>("https://blogy1.azurewebsites.net/api/Post/GetAllUserFavorites");
            var allPosts = await _httpClient.GetFromJsonAsync<List<PostViewModel>>("https://blogy1.azurewebsites.net/api/Post/AllPost");
            var allUsers = await _httpClient.GetFromJsonAsync<List<UserViewModel>>("https://blogy1.azurewebsites.net/api/Account/AllUser");
            var allCategories = await _httpClient.GetFromJsonAsync<List<CategoryViewModel>>("https://blogy1.azurewebsites.net/api/Post/GetAllCategories");

            var userFavorites = allFavoritePosts.Where(x => x.UserId == userId).ToList();

            var posts = new List<PostCategoryViewModel>();

            foreach (var item in postCategories)
            {
                var newPostCategory = new PostCategoryViewModel();

                var user = allUsers.FirstOrDefault(x => x.Id == item.UserId);
                item.User = user;
                newPostCategory.User = user;

                var category = allCategories.FirstOrDefault(x => x.Id == item.CategoryId);
                item.Category = category;
                newPostCategory.Category = category;

                var post = allPosts.FirstOrDefault(x => x.Id == item.PostId);
                if (!userFavorites.Any(x => x.PostId == item.PostId))
                {
                    continue;
                }
                if (post == null)
                {
                    continue;
                }
                else
                {
                    item.Post = post;
                    newPostCategory.Post = post;
                    newPostCategory.PostId = post.Id;
                    newPostCategory.UserId = item.UserId;
                    posts.Add(newPostCategory);
                }


            }


            var categories = new List<CategoryViewModel>();

            foreach (var category in allCategories)
            {
                foreach (var item in posts)
                {
                    if (item.Category.Name == category.Name)
                    {
                        if (!categories.Contains(category))
                        {
                            categories.Add(category);
                        }
                    }
                }
            }


            ViewBag.Categories = categories;

            posts = posts.ToList();

            return View(posts);
        }

        public async Task<IActionResult> GetAllIsDraft()
        {
            ViewBag.Logged = HttpContext.Session.GetString("IsLogged");
            ViewBag.Role = HttpContext.Session.GetString("Role");

            string userId = HttpContext.Session.GetString("UserId");

            //
            var postCategories = await _httpClient.GetFromJsonAsync<List<PostCategoryViewModel>>("https://blogy1.azurewebsites.net/api/Post/AllPostCategories");
            var allDraftPosts = await _httpClient.GetFromJsonAsync<List<PostViewModel>>("https://blogy1.azurewebsites.net/api/Post/GetAllIsDraft");
            var allUsers = await _httpClient.GetFromJsonAsync<List<UserViewModel>>("https://blogy1.azurewebsites.net/api/Account/AllUser");
            var allCategories = await _httpClient.GetFromJsonAsync<List<CategoryViewModel>>("https://blogy1.azurewebsites.net/api/Post/GetAllCategories");


            var posts = new List<PostCategoryViewModel>();

            foreach (var item in postCategories)
            {
                var newPostCategory = new PostCategoryViewModel();

                var user = allUsers.FirstOrDefault(x => x.Id == item.UserId);
                item.User = user;
                newPostCategory.User = user;

                var category = allCategories.FirstOrDefault(x => x.Id == item.CategoryId);
                item.Category = category;
                newPostCategory.Category = category;

                var post = allDraftPosts.FirstOrDefault(x => x.Id == item.PostId);
                if (post == null)
                {
                    continue;
                }
                else
                {
                    item.Post = post;
                    newPostCategory.Post = post;
                    newPostCategory.PostId = post.Id;
                    newPostCategory.UserId = item.UserId;
                    posts.Add(newPostCategory);
                }


            }


            var categories = new List<CategoryViewModel>();

            foreach (var category in allCategories)
            {
                foreach (var item in posts)
                {
                    if (item.Category.Name == category.Name)
                    {
                        if (!categories.Contains(category))
                        {
                            categories.Add(category);
                        }
                    }
                }
            }


            ViewBag.Categories = categories;

            posts = posts.Where(x => x.UserId == userId).ToList();

            return View(posts);
        }

        public async Task<IActionResult> ApproveDraft(Guid Id)
        {
            await _httpClient.PostAsJsonAsync("https://blogy1.azurewebsites.net/api/Post/UpdateDraft", Id);

            return RedirectToAction("GetAllIsDraft");

        }

        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ApprovePost(Guid Id)
        {
            await _httpClient.PostAsJsonAsync("https://blogy1.azurewebsites.net/api/Post/ApprovePost", Id);

            return RedirectToAction("ListUnapprovedPosts");
        }

        public async Task<IActionResult> DeletePost(Guid Id)
        {
            await _httpClient.PostAsJsonAsync("https://blogy1.azurewebsites.net/api/Post/DeletePost", Id);

            return RedirectToAction("ListUnapprovedPosts");
        }
        public async Task<IActionResult> DeleteDraft(Guid Id)
        {
            await _httpClient.PostAsJsonAsync("https://blogy1.azurewebsites.net/api/Post/DeletePost", Id);

            return RedirectToAction("GetAllIsDraft");
        }

        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ListRoles()
        {
            ViewBag.Logged = HttpContext.Session.GetString("IsLogged");
            ViewBag.Role = HttpContext.Session.GetString("Role");

            var roles = await _httpClient.GetFromJsonAsync<List<string>>("https://blogy1.azurewebsites.net/api/Account/GetAllRoles");

            return View(roles);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> DeleteRole(string selectedRole)
        {
            var roles = new List<string>() { selectedRole };


            if (!string.IsNullOrEmpty(selectedRole))
            {

                var result = await _httpClient.PostAsJsonAsync("https://blogy1.azurewebsites.net/api/Admin/DeleteRole", roles);
            }
            return RedirectToAction("ListRoles");
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> AddRole(string role)
        {
            var roles = new List<string>() { role };
            var result = await _httpClient.PostAsJsonAsync("https://blogy1.azurewebsites.net/api/Admin/Add-Role", roles);

            return RedirectToAction("ListRoles");
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> RemoveRole(string email, string roleName)
        {

            var updateRoleRequest = new UpdateRoleRequest { UserId = email, RoleName = roleName };
            var result = await _httpClient.PutAsJsonAsync("https://blogy1.azurewebsites.net/api/Admin/RemoveUserRole", updateRoleRequest);

            var roles = await _httpClient.GetFromJsonAsync<List<string>>("https://blogy1.azurewebsites.net/api/Account/GetUserRoleById?id=" + email);


            return Json(roles);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> GetUserRoles(string id)
        {

            var roles = await _httpClient.GetFromJsonAsync<List<string>>("https://blogy1.azurewebsites.net/api/Account/GetUserRoleById?id=" + id);

            return Json(roles);
        }

        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AssignRole()
        {
            ViewBag.Logged = HttpContext.Session.GetString("IsLogged");
            ViewBag.Role = HttpContext.Session.GetString("Role");

            var roles = await _httpClient.GetFromJsonAsync<List<string>>("https://blogy1.azurewebsites.net/api/Account/GetAllRoles");
            var users = await _httpClient.GetFromJsonAsync<List<UserViewModel>>("https://blogy1.azurewebsites.net/api/Account/AllUser");



            var userList = new List<string>() { };

            foreach (var user in users)
            {
                userList.Add(user.Email);
            }

            ViewBag.Roles = roles;
            ViewBag.Users = users;

            return View();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> AssignRole(string user, string role)
        {
            var updateRoleRequest = new UpdateRoleRequest { UserId = user, RoleName = role };
            var result = await _httpClient.PutAsJsonAsync("https://blogy1.azurewebsites.net/api/Admin/UpdateRole", updateRoleRequest);

            var roles = await _httpClient.GetFromJsonAsync<List<string>>("https://blogy1.azurewebsites.net/api/Account/GetUserRoleById?id=" + user);

            return Json(roles);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {

            var result = await _httpClient.PostAsJsonAsync("https://blogy1.azurewebsites.net/api/Post/DeleteCategory", id);


            return RedirectToAction("ListCategories");

        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> CreateCategory(string name)
        {
            var category = new CategoryViewModel { Name = name };
            if (ModelState.IsValid)
            {
                var result = await _httpClient.PostAsJsonAsync("https://blogy1.azurewebsites.net/api/Admin/CreateCategory", category);
                return RedirectToAction("ListCategories");
            }
            return RedirectToAction("ListCategories");
        }

        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ListCategories()
        {
            ViewBag.Logged = HttpContext.Session.GetString("IsLogged");
            ViewBag.Role = HttpContext.Session.GetString("Role");


            var categories = await _httpClient.GetFromJsonAsync<List<CategoryViewModel>>("https://blogy1.azurewebsites.net/api/Post/GetAllCategories");

            return View(categories);
        }



        public async Task<IActionResult> Post(Guid id)
        {
            ViewBag.Logged = HttpContext.Session.GetString("IsLogged");
            ViewBag.Role = HttpContext.Session.GetString("Role");

            var userId = HttpContext.Session.GetString("UserId");


            //var postt = await _httpClient.GetFromJsonAsync<List<PostResponse>>("https://blogy1.azurewebsites.net/api/Post/GetPost?id="+ id);

            var postCategories = await _httpClient.GetFromJsonAsync<List<PostCategoryViewModel>>("https://blogy1.azurewebsites.net/api/Post/AllPostCategories");
            var postComments = await _httpClient.GetFromJsonAsync<List<PostCommentViewModel>>("https://blogy1.azurewebsites.net/api/Post/GetAllPostComments");

            var users = await _httpClient.GetFromJsonAsync<List<UserViewModel>>("https://blogy1.azurewebsites.net/api/Account/AllUser");
            var posts = await _httpClient.GetFromJsonAsync<List<PostViewModel>>("https://blogy1.azurewebsites.net/api/Post/AllPost");
            var unapprovedPosts = await _httpClient.GetFromJsonAsync<List<PostViewModel>>("https://blogy1.azurewebsites.net/api/Post/AllUnApprovedPost");
            var drafts = await _httpClient.GetFromJsonAsync<List<PostViewModel>>("https://blogy1.azurewebsites.net/api/Post/GetAllIsDraft");
            var comments = await _httpClient.GetFromJsonAsync<List<CommentViewModel>>("https://blogy1.azurewebsites.net/api/Post/GetAllComments");
            var categories = await _httpClient.GetFromJsonAsync<List<CategoryViewModel>>("https://blogy1.azurewebsites.net/api/Post/GetAllCategories");
            var allPostTags = await _httpClient.GetFromJsonAsync<List<PostTagViewModel>>("https://blogy1.azurewebsites.net/api/Post/GetAllPostTags");
            var allFollowers = await _httpClient.GetFromJsonAsync<List<FollowerViewModel>>("https://blogy1.azurewebsites.net/api/Account/GetAllFollowers");
            var allFavoritePosts = await _httpClient.GetFromJsonAsync<List<FavoritePostViewModel>>("https://blogy1.azurewebsites.net/api/Post/GetAllUserFavorites");

            var p = new List<PostCategoryViewModel>();

            foreach (var item in postCategories)
            {
                var p2 = new PostCategoryViewModel();

                if (posts.FirstOrDefault(s => s.Id == item.PostId) == null)
                {
                    if (drafts.FirstOrDefault(s => s.Id == item.PostId) == null)
                    {
                        p2.Post = unapprovedPosts.FirstOrDefault(s => s.Id == item.PostId);
                    }
                    else
                    {
                        p2.Post = drafts.FirstOrDefault(s => s.Id == item.PostId);
                    }
                }
                else
                {
                    p2.Post = posts.FirstOrDefault(s => s.Id == item.PostId) == null ? drafts.FirstOrDefault(s => s.Id == item.PostId) : posts.FirstOrDefault(s => s.Id == item.PostId);
                }
                p2.PostId = item.PostId == null ? drafts.FirstOrDefault(s => s.Id == item.PostId).Id : item.PostId;
                p2.User = users.FirstOrDefault(s => s.Id == item.UserId);
                p2.UserId = item.UserId;
                p2.Category = categories.FirstOrDefault(s => s.Id == item.CategoryId);
                p2.CategoryId = item.CategoryId;

                p.Add(p2);
            }

            var post2 = p.FirstOrDefault(x => x.PostId == id);



            var c = new List<PostCommentViewModel>();

            foreach (var item in postComments)
            {
                if (item.PostId == id)
                {
                    var p2 = new PostCommentViewModel();
                    p2.User = users.FirstOrDefault(s => s.Id == item.UserId);
                    p2.UserId = item.UserId;
                    if(comments.FirstOrDefault(s => s.Id == item.CommentId) == null)
                    {
                        continue;
                    }
                    p2.Comment = comments.FirstOrDefault(s => s.Id == item.CommentId);
                    p2.CommentId = item.CommentId;
                    p2.PostId = item.PostId;
                    c.Add(p2);
                }
            }



            if (c == null)
            {
                c = new List<PostCommentViewModel>();
            }
            ViewBag.Comments = c;
            ViewBag.Categories = new List<string>() { post2 == null ? "" : post2.Category.Name };
            ViewBag.Tags = new List<string>() { "tag" };

            

            //hata veriyor
            if (post2 != null)
            {
                var follower = allFollowers.FirstOrDefault(x => x.AuthorId == post2.UserId && x.SubscriberId == userId);
                if(follower != null)
                {
                    ViewBag.Follower = "True";
                }
                else
                {
                    ViewBag.Follower = "False";
                }
                ViewBag.Author = post2.User.Firstname + " " + post2.User.Lastname;
                ViewBag.AuthorPicture = post2.User.Photo;
                ViewBag.Date = post2.Post.CreatedDate;
                ViewBag.AuthorId = post2.UserId;
            }
            else
            {
                ViewBag.Author = "";
                ViewBag.AuthorPicture = "";
                ViewBag.Date = "";
                ViewBag.Role = HttpContext.Session.GetString("Role");
                ViewBag.Tags = allPostTags;
                return View(new PostViewModel());
            }

            ViewBag.Role = HttpContext.Session.GetString("Role");
            ViewBag.Tags = allPostTags;

            var post = p.FirstOrDefault(x => x.PostId == id).Post;

            ViewBag.IsDraft = post.IsDraft.ToString();

            var favoritePost = allFavoritePosts.FirstOrDefault(x => x.PostId == id && x.UserId == userId);

            if(favoritePost != null)
            {
                ViewBag.IsFavorite = "True";
            }
            else
            {
                ViewBag.IsFavorite = "False";
            }

            return View(post);
        }

        [HttpGet]
        public async Task<IActionResult> AddPost(Guid Id)
        {
            ViewBag.Logged = HttpContext.Session.GetString("IsLogged");
            ViewBag.Role = HttpContext.Session.GetString("Role");


            var posts = await _httpClient.GetFromJsonAsync<List<PostViewModel>>("https://blogy1.azurewebsites.net/api/Post/AllPost");
            var post = posts.FirstOrDefault(x => x.Id == Id);




            if (post == null)
            {
                posts = await _httpClient.GetFromJsonAsync<List<PostViewModel>>("https://blogy1.azurewebsites.net/api/Post/GetAllIsDraft");
                post = posts.FirstOrDefault(x => x.Id == Id);
                if (post == null)
                {
                    posts = await _httpClient.GetFromJsonAsync<List<PostViewModel>>("https://blogy1.azurewebsites.net/api/Post/AllUnApprovedPost");
                    post = posts.FirstOrDefault(x => x.Id == Id);
                }
            }

            var allCategories = await _httpClient.GetFromJsonAsync<List<CategoryViewModel>>("https://blogy1.azurewebsites.net/api/Post/GetAllCategories");
            var allPostTags = await _httpClient.GetFromJsonAsync<List<PostTagViewModel>>("https://blogy1.azurewebsites.net/api/Post/GetAllPostTags");

            if (allPostTags != null && post != null)
            {
                var postTag = allPostTags.Where(x => x.PostId == post.Id).ToList();

                List<string> tagList = new List<string>();

                foreach (var item in postTag)
                {
                    tagList.Add(item.Tag.Title);
                }

                ViewBag.Tags = tagList;

            }
            else
            {
                post = new PostViewModel();
                ViewBag.Tags = new List<string>();
            }

            ViewBag.Categories = allCategories;

            return View(post);
        }


        [HttpPost]
        public async Task<IActionResult> AddPost(PostViewModel post, string tags, IFormFile photo, List<Guid> categories = null)
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

            post.Photo = photoPath;

            var userId = HttpContext.Session.GetString("UserId");

            var allCategories = await _httpClient.GetFromJsonAsync<List<CategoryViewModel>>("https://blogy1.azurewebsites.net/api/Post/GetAllCategories");
            var allPostTags = await _httpClient.GetFromJsonAsync<List<PostTagViewModel>>("https://blogy1.azurewebsites.net/api/Post/GetAllPostTags");

            var postTag = allPostTags.Where(x => x.PostId == post.Id).ToList();

            List<string> tagList = new List<string>();

            if (tags != null)
            {
                tagList = tags.Trim().Split(' ').ToList();
            }


            ViewBag.Categories = allCategories;

            var postRequest = new AddPostRequest();
            postRequest.Post = post;
            postRequest.UserId = userId;
            postRequest.CategoryIds = categories;
            postRequest.Tags = tagList;
            postRequest.PostTagIds = new List<Guid>();

            foreach (var item in postTag)
            {
                postRequest.PostTagIds.Add(item.Id);
            }

            if (post.Id == Guid.Empty)
            {
                var result = await _httpClient.PostAsJsonAsync("https://blogy1.azurewebsites.net/api/Post/AddPost", postRequest);
            }
            else
            {
                var result = await _httpClient.PostAsJsonAsync("https://blogy1.azurewebsites.net/api/Post/UpdatePost", postRequest);
            }



            return RedirectToAction("Index");
        }

        public async Task<IActionResult> AddToFavorites(Guid id)
        {

            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index");
            }

            var favoritePost = new FavoritePostRequest() { PostId = id, UserId = userId };

            var result = await _httpClient.PostAsJsonAsync("https://blogy1.azurewebsites.net/api/Post/AddToFavorites", favoritePost);

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> AddCommentReply(string message, Guid postId, Guid parentId)
        {

            var userId = HttpContext.Session.GetString("UserId");

            if (userId == null)
            {
                return RedirectToAction("Post", "Home", new { id = postId });
            }

            var comment = new CommentViewModel()
            {
                Content = message,
                IsParent = false,
                ParentId = parentId
            };

            var postRequest = new AddCommentRequest();
            postRequest.Comment = comment;
            postRequest.PostId = postId;
            postRequest.UserId = HttpContext.Session.GetString("UserId");

            var result = await _httpClient.PostAsJsonAsync("https://blogy1.azurewebsites.net/api/Post/AddComment", postRequest);

            return RedirectToAction("Post", new { id = postId });

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(string message, Guid postId)
        {


            var userId = HttpContext.Session.GetString("UserId");

            if (userId == null)
            {
                return RedirectToAction("Post", "Home", new { id = postId });
            }

            var comment = new CommentViewModel()
            {
                Content = message,
                IsParent = true,
            };

            var postRequest = new AddCommentRequest();
            postRequest.Comment = comment;
            postRequest.PostId = postId;
            postRequest.UserId = HttpContext.Session.GetString("UserId");

            var result = await _httpClient.PostAsJsonAsync("https://blogy1.azurewebsites.net/api/Post/AddComment", postRequest);

            return RedirectToAction("Post", new { id = postId });

        }

        public async Task<IActionResult> DeleteComment(Guid id)
        {

            var result = await _httpClient.PostAsJsonAsync("https://blogy1.azurewebsites.net/api/Post/DeleteComment", id);


            return RedirectToAction("ListUnapprovedComments");
        }

        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> EditComment(Guid id, string content)
        {
            var comment = new CommentViewModel()
            {
                Id = id,
                Content = content
            };
            var result = await _httpClient.PostAsJsonAsync("https://blogy1.azurewebsites.net/api/Post/UpdateComment", comment);


            return RedirectToAction("ListUnapprovedComments");
        }

        public async Task<IActionResult> GetUserNotifications()
        {
            ViewBag.Logged = HttpContext.Session.GetString("IsLogged");
            ViewBag.Role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetString("UserId");
            var response = await _httpClient.GetFromJsonAsync<List<NotificationViewModel>>("https://blogy1.azurewebsites.net/api/Post/GetNotification");
            var notifications = response.Where(x => x.UserId == userId).ToList();

            return View(notifications);
        }

        public async Task<IActionResult> EditNotification(List<Guid> id)
        { 
            var result = await _httpClient.PostAsJsonAsync("https://blogy1.azurewebsites.net/api/Post/UpdateNotification", id);

            return RedirectToAction("GetUserNotifications");
        }


        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ListUnapprovedComments()
        {
            ViewBag.Logged = HttpContext.Session.GetString("IsLogged");
            ViewBag.Role = HttpContext.Session.GetString("Role");


            var postComments = await _httpClient.GetFromJsonAsync<List<PostCommentViewModel>>("https://blogy1.azurewebsites.net/api/Post/GetAllPostComments");

            var users = await _httpClient.GetFromJsonAsync<List<UserViewModel>>("https://blogy1.azurewebsites.net/api/Account/AllUser");
            var posts = await _httpClient.GetFromJsonAsync<List<PostViewModel>>("https://blogy1.azurewebsites.net/api/Post/AllPost");
            var comments = await _httpClient.GetFromJsonAsync<List<CommentViewModel>>("https://blogy1.azurewebsites.net/api/Post/GetAllUnApprovedComments");




            var c = new List<PostCommentViewModel>();

            foreach (var item in postComments)
            {
                var p2 = new PostCommentViewModel();
                p2.User = users.FirstOrDefault(s => s.Id == item.UserId);
                p2.UserId = item.UserId;
                p2.Comment = comments.FirstOrDefault(s => s.Id == item.CommentId);
                p2.CommentId = item.CommentId;

                if (p2.Comment != null && !p2.Comment.IsApproved)
                {
                    c.Add(p2);
                }
            }



            if (c == null)
            {
                c = new List<PostCommentViewModel>();
            }
            ViewBag.Comments = c;
            ViewBag.Tags = new List<string>() { "tag" };
            ViewBag.Role = HttpContext.Session.GetString("Role");


            return View();
        }

        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ApproveComment(Guid id)
        {

            await _httpClient.PostAsJsonAsync("https://blogy1.azurewebsites.net/api/Post/ApproveComment", id);

            return RedirectToAction("ListUnapprovedComments");
        }
        public async Task<IActionResult> Privacy()
        {
            return RedirectToAction("ListUnapprovedComments");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
