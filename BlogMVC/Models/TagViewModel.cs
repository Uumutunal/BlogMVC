namespace BlogMVC.Models
{
    public class TagViewModel
    {
        public string Title { get; set; }
        public List<PostTagViewModel> PostTags { get; set; }
    }
}
