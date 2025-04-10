using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

internal class Session
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SessionPK { get; set; }
    public int UserId { get; set; }
    public string SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
}