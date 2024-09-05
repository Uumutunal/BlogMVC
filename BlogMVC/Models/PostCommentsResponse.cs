using Microsoft.AspNet.Identity;
using Microsoft.Extensions.Hosting;
using System.Xml.Linq;

namespace BlogMVC.Models
{
    public class PostCommentsResponse
    {
        public List<PostViewModel> Posts { get; set; }
        public List<UserViewModel> Users { get; set; }
        public List<CommentViewModel> Comments { get; set; }
    }
}
