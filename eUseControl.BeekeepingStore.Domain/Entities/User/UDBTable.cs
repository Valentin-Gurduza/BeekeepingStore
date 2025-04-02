using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eUseControl.BeekeepingStore.Domain.Entities.User
{
    public class UDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "UserName")]
        [StringLength(30, MinimumLength = 5, ErrorMessage = "User cannot be longer than 30 characters")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Password")]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "Password cannot be longer than 50 characters")]

        public string Password { get; set; }

        [Required]
        [Display(Name = "Email")]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "Email cannot be longer than 50 characters")]
        public string Email { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public string UserIP { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime RegistrationDateTime { get; set; }

        [Display(Name = "Last Login")]
        public DateTime LoginDateTime { get; set; }


    }
}
