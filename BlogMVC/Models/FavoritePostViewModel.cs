namespace BlogMVC.Models
{
    public class FavoritePostViewModel
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public UserViewModel User { get; set; }
        public Guid PostId { get; set; }
        public PostViewModel Post { get; set; }
    }
}
