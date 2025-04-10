using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

internal class UserActivity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ActivityId { get; set; }
    public int UserId { get; set; }
    public string Activity { get; set; }
    public DateTime CreatedAt { get; set; }
}
