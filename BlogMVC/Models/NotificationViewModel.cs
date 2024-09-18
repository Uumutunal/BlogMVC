using Microsoft.AspNet.Identity;

namespace BlogMVC.Models
{
    public class NotificationViewModel
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public UserViewModel User { get; set; }
        public string UserId { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime? CreatedDate { get; set; }
    }
}
