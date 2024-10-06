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

		public async Task<IEnumerable<Job>> GetJobsAsync()
		{
			try
			{
				var JobData = await _context.Jobs
											.FromSqlRaw("EXEC [dbo].[GetJobs]")
											.ToListAsync();

				return JobData;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while retrieving jobs.");
				throw;
			}
		}

		public async Task<bool> DeleteJobAsync(int jobId)
		{
			var successParam = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
			var jobIdParam = new SqlParameter("@JobId", jobId);

			try
			{
				await _context.Database.ExecuteSqlRawAsync("EXEC [dbo].DeleteJob @JobId, @Success OUTPUT", jobIdParam, successParam);

				return (bool)successParam.Value;
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		public async Task<bool> UpdateJobAsync(Job job)
		{
			try
			{
				var jidParam = new SqlParameter("@Jid", job.id.HasValue ? (object)job.id.Value : DBNull.Value);
				var jNameParam = new SqlParameter("@JName", job.jobName ?? (object)DBNull.Value);
				var jDescriptionParam = new SqlParameter("@JDescription", job.jobDescription ?? (object)DBNull.Value);
				var locationParam = new SqlParameter("@Location", job.location ?? (object)DBNull.Value);
				var statusParam = new SqlParameter("@Status", job.status == 1 ? (object)1 : (object)0);
				var experienceParam = new SqlParameter("@Experience", job.experience ?? (object)DBNull.Value);


				var successParam = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };

				await _context.Database.ExecuteSqlRawAsync(
				   "EXEC [dbo].[UpdateJob] @Jid, @JName, @JDescription, @Location, @Status, @Experience, @Success OUTPUT",
				   jidParam, jNameParam, jDescriptionParam, locationParam, statusParam, experienceParam, successParam
			   );

				
				 return (bool)successParam.Value;


			}
			catch (Exception ex)
			{
				throw;

			}
		}

		public async Task<IEnumerable<Job>> GetJobsAsyncActive(int pageNumber, int pageSize)
		{
			try
			{
				var jobs = await _context.Jobs
					.FromSqlRaw("EXEC [dbo].[GetJobsCareer] @PageNumber, @PageSize",
						new SqlParameter("@PageNumber", pageNumber),
						new SqlParameter("@PageSize", pageSize))
					.ToListAsync();

				return jobs;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while retrieving active jobs.");
				throw;
			}
		}

		public Task<int> GetOpeningsCountAsync()
		{
			throw new NotImplementedException();
		}
	}
}
