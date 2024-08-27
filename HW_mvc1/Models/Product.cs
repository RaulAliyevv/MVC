namespace HW_mvc1.Models
{
    public class Product:BaseEntity
    {
		public string Name { get; set; }
		public string Description { get; set; }
		public decimal Price { get; set; }
		public string SKU { get; set; }

		public int CategoryId { get; set; }
		public Category Category { get; set; }


        public ICollection<ProductImage> ProductImages { get; set; }
        public ICollection<ProductTag>? ProductTags { get; set; }
		public ICollection<ProductColor>? ProductColors { get; set; }
		public ICollection<ProductSize>? ProductSizes { get; set; }

	}
}
