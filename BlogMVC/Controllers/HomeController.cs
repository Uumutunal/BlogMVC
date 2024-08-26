using BlogMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BlogMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
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


			List<PostViewModel> posts = new List<PostViewModel>();

			posts.Add(new PostViewModel() { Content = "Content", Title = "Title" });

			return View(posts);
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
