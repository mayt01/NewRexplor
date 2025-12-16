using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Rexplor.Models
{
    public class MainPageVoice
    {
        public int Id { get; set; }
        public string Name { get; set; }    // Optional, to store the sender name
        public string Caption { get; set; }   // Optional, to store the caption
        public byte[] VoiceData { get; set; }   // To store the image as byte array


        //[MaxLength(5 * 1024 * 1024)]  // Example: Max file size of 5MB
        //[RegularExpression(@"^.*\.(jpg|jpeg|png|gif)$", ErrorMessage = "Invalid file format. Only images are allowed.")]

        [NotMapped] // Ignore the ImageFile property
        [Required]
        [DataType(DataType.Upload)]
        public IFormFile VoiceFile { get; set; } // For file uploads only
    }
}
