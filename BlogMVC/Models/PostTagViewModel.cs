namespace BlogMVC.Models
{
    public class PostTagViewModel
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public PostViewModel Post { get; set; }
        public Guid TagId { get; set; }
        public TagViewModel Tag { get; set; }
    }
}
