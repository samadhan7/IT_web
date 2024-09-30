using System.ComponentModel.DataAnnotations;

namespace GTL.Models
{
	public class Applications
	{
		[Key]
		public string? Name { get; set; }
		public string? Contact { get; set; }
		public string? Address { get; set; }
		public string? Email { get; set; }
		public string? CoverLetter { get; set; }
		public string? Resume { get; set; }

	}
}
