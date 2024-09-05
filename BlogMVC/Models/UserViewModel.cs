namespace BlogMVC.Models
{
	public class UserViewModel
	{
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string Photo { get; set; } = string.Empty;

    }
}
