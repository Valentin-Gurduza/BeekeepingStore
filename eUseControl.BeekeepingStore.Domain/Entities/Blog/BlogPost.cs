using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eUseControl.BeekeepingStore.Domain.Entities.Blog
{
    [Table("BlogPosts")]
    public class BlogPost
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BlogPostId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [MaxLength(500)]
        public string Summary { get; set; }

        [MaxLength(255)]
        public string FeaturedImage { get; set; }

        [Required]
        public DateTime PublishDate { get; set; }

        [MaxLength(100)]
        public string Author { get; set; }

        public bool IsPublished { get; set; }

        [MaxLength(50)]
        public string Category { get; set; }

        public string Tags { get; set; }

        public int ViewCount { get; set; }

        [MaxLength(200)]
        public string Slug { get; set; }

        // Navigation property for comments
        public virtual ICollection<BlogComment> Comments { get; set; }
    }
}