using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rexplor.Models
{
    public class MainPageSliderImage
    {
        public int Id { get; set; }
        public string Caption { get; set; }    // Optional, to store the file name
        public byte[] ImageData { get; set; }   // To store the image as byte array


        //[MaxLength(5 * 1024 * 1024)]  // Example: Max file size of 5MB
        //[RegularExpression(@"^.*\.(jpg|jpeg|png|gif)$", ErrorMessage = "Invalid file format. Only images are allowed.")]

        [NotMapped] // Ignore the ImageFile property
        [Required]
        [DataType(DataType.Upload)]
        public IFormFile ImageFile { get; set; } // For file uploads only
    }
}
