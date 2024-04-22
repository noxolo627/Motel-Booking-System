using Microsoft.AspNetCore.Identity;

namespace MotelBooking.Repository.Interface
{
    public interface ITokenRepository
    {
        public string CreateJwtToken(IdentityUser user, List<string> roles);
    }
}
