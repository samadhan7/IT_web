using System.ComponentModel.DataAnnotations;

namespace GTL.Models
{
	public class Applications
	{
		[Key]
		public int? Id { get; set; }
		public string? Name { get; set; }
		public string? Contact { get; set; }
		public string? Address { get; set; }
		public string? Email { get; set; }
		public string? CoverLetter { get; set; }
		public string? Resume { get; set; }
		public int? job_id { get; set; }

		public string? job_name { get; set; }

	}
}
