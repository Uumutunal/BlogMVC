using System.Reflection.Metadata.Ecma335;

namespace BlogMVC.Models
{
    public class FollowerViewModel
    {
        public Guid Id { get; set; }
        public string AuthorId { get; set; }
        public string SubscriberId { get; set; }
    }
}
