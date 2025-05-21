using System;
using System.ComponentModel.DataAnnotations;

namespace eUseControl.BeekeepingStore.Models
{
    public class BlogCommentModel
    {
        public int BlogPostId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Your Name")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Your Email")]
        [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Comment content is required")]
        [Display(Name = "Comment")]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; }

        // Optional parent comment ID (for replies)
        public int? ParentCommentId { get; set; }
    }
}