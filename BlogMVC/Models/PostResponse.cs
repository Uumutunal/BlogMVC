namespace BlogMVC.Models
{
    public class PostResponse
    {
        public PostViewModel Post { get; set; }
        public CommentViewModel Comment { get; set; }
        public UserViewModel User { get; set; }
        public CategoryViewModel Category { get; set; }
    }
}
