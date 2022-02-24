using System.ComponentModel.DataAnnotations;

namespace app.Model.DTOs.Requests
{
    public class UserRegistationDto
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
