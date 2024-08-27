using HW_mvc1.Models;

namespace HW_mvc1.Areas.ProniaAdminPanel.ViewModels
{
	public class GetAdminProductVM
	{
        public int Id { get; set; }
        public string Name { get; set; }
		public decimal Price { get; set; }
		public string CategoryName { get; set; }
		//public string ColorName { get; set; }
		//public string SizeName { get; set; }
		public string Image { get; set; }
	}
}
