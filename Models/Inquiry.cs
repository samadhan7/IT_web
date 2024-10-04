using System.ComponentModel.DataAnnotations;

namespace GTL.Models
{
	public class Inquiry
	{
		[Key]
		public int? id { get; set; }
		public string? name { get; set; }
		public string? email { get; set; }
		public string? contact { get; set; }
		public string? subject { get; set; }
		public string? message { get; set; }
		public DateTime? last_modified { get; set; }
	}
}
