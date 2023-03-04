using System.ComponentModel.DataAnnotations;

namespace ChatGptByNet.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        public string? UserName { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }

        [Required]
        public string? PassWord { get; set; }

        public int? Status { get; set; } = 1;

        public bool? IsDelete { get; set; } = false;

        public DateTime? InsertTime { get; set; }

        public int? Count { get; set; } = 10;

        public string? PhoneNumber { get; set; }
    }
}
