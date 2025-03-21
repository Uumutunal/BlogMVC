﻿namespace BlogMVC.Models
{
    public class AddPostRequest
    {
        public PostViewModel Post { get; set; }
        public List<Guid> CategoryIds { get; set; } = null;
        public string UserId { get; set; }
        public List<string> Tags { get; set; }
        public List<Guid> PostTagIds { get; set; }
    }
}
