using System.ComponentModel.DataAnnotations;

namespace HW_mvc1.Models
{
    public class Category:BaseEntity
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(30, ErrorMessage = "Length must be <= 30")]
        public string Name { get; set; }
        public ICollection<Product>? Products { get; set; }
    }
}
