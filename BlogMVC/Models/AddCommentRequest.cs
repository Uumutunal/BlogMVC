namespace BlogMVC.Models
{
    public class AddCommentRequest
    {
        public CommentViewModel Comment { get; set; }
        public string UserId { get; set; }
        public Guid PostId { get; set; }
    }
}
