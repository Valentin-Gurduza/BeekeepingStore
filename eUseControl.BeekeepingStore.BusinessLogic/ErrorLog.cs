using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

internal class ErrorLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ErrorId { get; set; }
    public string Message { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorSource { get; set; }
    public string StackTrace { get; set; }
    public string ErrorStackTrace { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ErrorDate { get; set; }
}