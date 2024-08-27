using System.ComponentModel.DataAnnotations;

namespace HW_mvc1.Areas.ProniaAdminPanel.ViewModels
{
	public class CreateSlideVM
	{
		public string Title { get; set; }
		public string Subtitle { get; set; }
		public string Description { get; set; }
		public int Order { get; set; }
		[Required]
		public IFormFile Photo { get; set; }
	}
}
