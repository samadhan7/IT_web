using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GTL.Models
{
	public class Job
	{
		[Key]
		public int? id { get; set; }

		[Column("job_name")]
		public string? jobName { get; set; }

		[Column("job_description")]
		public string? jobDescription { get; set; }

		public string? location { get; set; }

		public string? experience { get; set; }

		public int status { get; set; }
	}
}
