using Microsoft.Extensions.Hosting;

namespace BlogMVC.Models
{
    public class PostCategoryViewModel
    {
        public Guid PostId { get; set; }
        public PostViewModel Post { get; set; }
        public Guid CategoryId { get; set; }
        public CategoryViewModel Category { get; set; }
    }
}
