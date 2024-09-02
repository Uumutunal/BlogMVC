using Microsoft.AspNet.Identity;
using Microsoft.Extensions.Hosting;
using System.Xml.Linq;

namespace BlogMVC.Models
{
    public class PostCommentViewModel
    {
        public Guid PostId { get; set; }
        public PostViewModel Post { get; set; }
        public Guid CommentId { get; set; }
        public CommentViewModel Comment { get; set; }
        public string UserId { get; set; }
        public UserViewModel User { get; set; }
    }
}
