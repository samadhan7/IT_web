using System.ComponentModel.DataAnnotations;

namespace GTL.Models
{
	public class Job
	{
		[Key]
		public string? id { get; set; }

		public string? jobName { get; set; }

		public string? jobDescription { get; set; }

		public string? location { get; set; }

		public string? experience { get; set; }

		public int status { get; set; }
	}
}
