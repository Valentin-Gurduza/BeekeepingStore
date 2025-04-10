using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

internal class ErrorLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ErrorId { get; set; }
    public string Message { get; set; }
    public string StackTrace { get; set; }
    public DateTime CreatedAt { get; set; }
}