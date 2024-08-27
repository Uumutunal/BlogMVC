namespace BlogMVC.Models
{
    public class PostViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Photo { get; set; } = string.Empty;
        public bool IsApproved { get; set; } = false;
        public int Likes { get; set; } = 0;
    }
}
