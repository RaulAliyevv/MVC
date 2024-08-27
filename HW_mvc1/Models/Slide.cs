using System.ComponentModel.DataAnnotations.Schema;

namespace HW_mvc1.Models
{
    public class Slide : BaseEntity
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int Order { get; set; }
        
    }
}
