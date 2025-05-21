using System;

namespace eUseControl.BeekeepingStore.Domain.Entities.User
{
    public class UProfileData
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? RegisterDate { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? Last_Login { get; set; }
        public string ProfileImage { get; set; }
        public string ProfilePicture { get; set; }
        public int Level { get; set; }
        public string UserIp { get; set; }
    }
}
