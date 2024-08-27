using HW_mvc1.Models;
using HW_mvc1.Utilities.Enums;

namespace HW_mvc1.Utilities.Extensions
{
	public static class FileValidator
	{
		public static bool ValidateType(this IFormFile file, string type)
		{
			if (file.ContentType .Contains(type))
			{
				return true;
			}
			return false;
		}
		public static bool ValidateSize(this IFormFile file, FileSize fileSize, int size)
		{
			switch(fileSize)
			{
				case FileSize.KB:
					return file.Length < 2 * 1024;
				case FileSize.MB:
					return file.Length < 2 * 1024 * 1024;
			}
			return false;
		}
		public static async Task<string> CreateFileAsync(this IFormFile file, params string[] roots)
		{
			string filename = String.Concat(Guid.NewGuid().ToString(), file.FileName);
			string path = string.Empty;

			for (int i = 0; i < roots.Length; i++)
			{
				path = Path.Combine(path, roots[i]);
			}
			path = Path.Combine(path, filename);

			using (FileStream filestream = new FileStream(path, FileMode.Create))
			{
				await file.CopyToAsync(filestream);
			}
			return filename;
		}

		public static void DeleteFile(this string filename, params string[] roots)
		{
			string path = string.Empty;

			for (int i = 0; i < roots.Length; i++)
			{
				path = Path.Combine(path, roots[i]);
			}
			path = Path.Combine(path, filename);



			if (System.IO.File.Exists(path))
			{
				System.IO.File.Delete(path);
			}
		}

	}
}
