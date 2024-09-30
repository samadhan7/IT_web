using GTL.Models;
using GTL.Repo.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace GTL.Repo.Class
{
	public class Jobs : IJobs
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<Jobs> _logger;

		public Jobs(ApplicationDbContext context, ILogger<Jobs> logger)
		{
			_context = context;
			_logger = logger;
		}
		async public Task<string> AddJobAsync(Job job)
		{
			try
			{
				var jNameParam = new SqlParameter("@JName", job.jobName ?? (object)DBNull.Value);
				var jDescriptionParam = new SqlParameter("@JDescription", job.jobDescription?? (object)DBNull.Value);
				var locationParam = new SqlParameter("@Location", job.location ?? (object)DBNull.Value);
				var statusParam = new SqlParameter("@Status", job.status == 1 ? (object)1 : (object)0);
				var experienceParam = new SqlParameter("@Experience", job.experience ?? (object)DBNull.Value);


				var responseMessageParam = new SqlParameter("@ResponseMessage", SqlDbType.NVarChar, 256) { Direction = ParameterDirection.Output };

				await _context.Database.ExecuteSqlRawAsync(
				   "EXEC [dbo].[InsertJob] @JName, @JDescription, @Location, @Status, @Experience, @ResponseMessage OUTPUT",
				   jNameParam, jDescriptionParam, locationParam, statusParam, experienceParam, responseMessageParam
			   );

				var ResponseMessage = responseMessageParam.Value == DBNull.Value ? string.Empty : (string)responseMessageParam.Value;

				return ResponseMessage;


			}
			catch (Exception ex)
			{
				return "Internal Server Error :" + ex.Message;

			}
		}

	


	}
}
