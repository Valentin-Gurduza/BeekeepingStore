using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eUseControl.BeekeepingStore.Domain.Entities.Blog
{
    [Table("BlogComments")]
    public class BlogComment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CommentId { get; set; }

        [Required]
        public int BlogPostId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime CommentDate { get; set; }

        public bool IsApproved { get; set; }

        // Parent comment ID for replies
        public int? ParentCommentId { get; set; }

        // Navigation properties
        [ForeignKey("BlogPostId")]
        public virtual BlogPost BlogPost { get; set; }

        [ForeignKey("ParentCommentId")]
        public virtual BlogComment ParentComment { get; set; }
    }
}