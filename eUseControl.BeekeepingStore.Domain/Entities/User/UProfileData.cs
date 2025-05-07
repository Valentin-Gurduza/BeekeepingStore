using System;

namespace eUseControl.BeekeepingStore.BusinessLogic.Interfaces
{
    public class UProfileData
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? RegisterDate { get; set; }
        public DateTime? LastLogin { get; set; }
        public string ProfilePicture { get; set; }
    }
}
