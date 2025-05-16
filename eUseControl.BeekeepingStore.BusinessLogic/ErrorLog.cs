using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

internal class ErrorLog
{
    private static readonly DateTime MinSqlDateTime = new DateTime(1753, 1, 1);

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ErrorId { get; set; }

    public string Message { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorSource { get; set; }
    public string StackTrace { get; set; }
    public string ErrorStackTrace { get; set; }

    private DateTime _createdAt = DateTime.Now;
    public DateTime CreatedAt
    {
        get { return _createdAt; }
        set { _createdAt = value < MinSqlDateTime ? MinSqlDateTime : value; }
    }

    private DateTime _errorDate = DateTime.Now;
    public DateTime ErrorDate
    {
        get { return _errorDate; }
        set { _errorDate = value < MinSqlDateTime ? MinSqlDateTime : value; }
    }

    public ErrorLog()
    {
        // Asigură-te că toate proprietățile DateTime au valori implicite valide
        CreatedAt = DateTime.Now;
        ErrorDate = DateTime.Now;
    }
}