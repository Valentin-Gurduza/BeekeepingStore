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
    [Table("UDBTables")]
    public class UDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Username")]
        [StringLength(30, MinimumLength = 5, ErrorMessage = "Username cannot be longer than 30 characters")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Password")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password cannot be longer than 50 characters")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Email")]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "Email cannot be longer than 50 characters")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Last_Login")]
        public DateTime Last_Login { get; set; }

        [Required]
        [Display(Name = "UserIp")]
        [StringLength(30, MinimumLength = 7, ErrorMessage = "IP cannot be longer than 30 characters")]
        public string UserIp { get; set; }

        [Required]
        [Display(Name = "Level")]
        public int Level { get; set; }

        [Display(Name = "RegisterDate")]
        public DateTime RegisterDate { get; set; }

        [Display(Name = "PhoneNumber")]
        [StringLength(20, ErrorMessage = "Phone number cannot be longer than 20 characters")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Address")]
        [StringLength(200, ErrorMessage = "Address cannot be longer than 200 characters")]
        public string Address { get; set; }

        [Display(Name = "ProfilePicture")]
        [StringLength(500, ErrorMessage = "Profile picture URL cannot be longer than 500 characters")]
        public string ProfilePicture { get; set; }
    }
}
