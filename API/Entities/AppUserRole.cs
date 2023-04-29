using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    // represent a join table between AppUser and AppRole
    public class AppUserRole : IdentityUserRole<int>
    {
        public AppUser User { get; set; }
        public AppRole Role { get; set; }
    }
}