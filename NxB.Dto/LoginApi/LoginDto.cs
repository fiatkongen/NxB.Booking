using System.ComponentModel.DataAnnotations;

namespace NxB.Dto.LoginApi
{
    public class LoginDto
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string AccessCardId { get; set; }

        [Required]
        public string ClientId { get; set; }
    }
}
