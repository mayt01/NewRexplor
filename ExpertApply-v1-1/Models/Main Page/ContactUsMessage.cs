using System.ComponentModel.DataAnnotations;

namespace Rexplor.Models
{
    public class ContactUsMessage
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(500)]
        public string Message { get; set; }

        public DateTime DateSubmitted { get; set; } = DateTime.Now;
    }
}
