using System.ComponentModel.DataAnnotations;

namespace GTL.Models
{
    public class LoginRequest
    {
        [Key]
        public string UserId { get; set; }

        public string email { get; set; }

        public string password { get; set; }

        public string Role { get; set; }
    }
}
