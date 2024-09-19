namespace BlogMVC.Models
{
    public class CommentViewModel
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsApproved { get; set; }

        public Guid? ParentId { get; set; }
        public bool? IsParent { get; set; }
    }
}
