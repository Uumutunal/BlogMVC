namespace BlogMVC.Models
{
    public class PostViewModel
    {
        public Guid? Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public bool? IsApproved { get; set; }
        public string? Author { get; set; }
    }
}
