using BlogMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http;

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


            var allPosts = await _httpClient.GetFromJsonAsync<List<PostViewModel>>("https://localhost:7230/api/Post/AllPost");
            var allCategories = await _httpClient.GetFromJsonAsync<List<PostCategoryViewModel>>("https://localhost:7230/api/Post/AllPostCategories");

            return View(allPosts);
        }


		public IActionResult ListUnapprovedPosts()
		{
            ViewBag.Logged = HttpContext.Session.GetString("IsLogged");
            ViewBag.IsAdmin = HttpContext.Session.GetString("IsAdmin");

            List<PostViewModel> posts = new List<PostViewModel>();

            posts.Add(new PostViewModel() { Content = "Content" , Title = "Title"});

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
