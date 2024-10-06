using GTL.Models;
using GTL.Repo.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
namespace GTL.Repo.Class
{
	public class Application : IApplication
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<Application> _logger;

		public Application(ApplicationDbContext context, ILogger<Application> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task<bool> CheckApplicationAsync(string email, string phone)
		{
			try
			{
				var EmailParam = new SqlParameter("@Email", email ?? (object)DBNull.Value);
				var ContactParam = new SqlParameter("@Contact", phone ?? (object)DBNull.Value);
				var ExistsParam = new SqlParameter("@Exists", SqlDbType.Bit) { Direction = ParameterDirection.Output };

				await _context.Database.ExecuteSqlRawAsync(
				   "EXEC [dbo].[CheckApplication] @Contact, @Email, @Exists OUTPUT",
				    ContactParam, EmailParam, ExistsParam
			   );

				return (bool)ExistsParam.Value;
			}
			catch(Exception ex) {
				throw;
			}
		}


		public async Task<string> DeleteApplicationAsync(int id)
		{
			string resumeFilePath = string.Empty;

			// Prepare parameter for getting resume file path
			var idParam = new SqlParameter("@Id", id);
			var resumeFilePathParam = new SqlParameter("@ResumeFilePath", SqlDbType.NVarChar, 255)
			{
				Direction = ParameterDirection.Output
			};

			// First, get the resume file path
			await _context.Database.ExecuteSqlRawAsync("EXEC [dbo].[GetResumeFilePath] @Id, @ResumeFilePath OUTPUT", idParam, resumeFilePathParam);

			// Retrieve the file path from output parameter
			resumeFilePath = resumeFilePathParam.Value as string;

			var successParam = new SqlParameter("@Success", SqlDbType.NVarChar, 256) { Direction = ParameterDirection.Output };

			try
			{

				bool fileDeletedSuccessfully = false; // Track if the file was deleted successfully

				if (!string.IsNullOrEmpty(resumeFilePath))
				{
					string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", resumeFilePath.Replace("wwwroot\\", ""));

					if (System.IO.File.Exists(fullPath))
					{
						System.IO.File.Delete(fullPath); // Attempt to delete the file
						fileDeletedSuccessfully = true; // Mark as successful if no exception occurs
					}
				}

				if (fileDeletedSuccessfully)
				{

					// Execute stored procedure to delete application record
					await _context.Database.ExecuteSqlRawAsync("EXEC [dbo].[DeleteApplication] @Id, @Success OUTPUT", idParam, successParam);

					var ResponseMessage = successParam.Value == DBNull.Value ? string.Empty : (string)successParam.Value;

					return ResponseMessage;
				}
			}
			catch (Exception ex)
			{
				// Handle exceptions as needed
				return $"Error: {ex.Message}";
				throw;
			}
			return "Internal Server Error.";
		}

		public async Task<IEnumerable<Applications>> GetApplicationsAsync()
		{
			try
			{
				var ApplicationsData = await _context.applications
											.FromSqlRaw("EXEC [dbo].[GetApplications]")
											.ToListAsync();

				return ApplicationsData;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while retrieving applications.");
				throw;
			}
		}

		public Task<int> GetApplicationsCountAsync()
		{
			throw new NotImplementedException();
		}

		async Task<string> IApplication.AddApplicationAsync(Applications applications)
		{

			try
			{
				var NameParam = new SqlParameter("@Name", applications.Name ?? (object)DBNull.Value);
				var ContactParam = new SqlParameter("@Contact", applications.Contact ?? (object)DBNull.Value);
				var AddressParam = new SqlParameter("@Address", applications.Address ?? (object)DBNull.Value);
				var EmailParam = new SqlParameter("@Email", applications.Email ?? (object)DBNull.Value);
				var CLParam = new SqlParameter("@CoverLetter", applications.CoverLetter ?? (object)DBNull.Value);
				var ResumeParam = new SqlParameter("@Resume", applications.Resume ?? (object)DBNull.Value);
				var jidParam = new SqlParameter("@Jid", applications.job_id.HasValue ? (object)applications.job_id.Value : DBNull.Value);


				var responseMessageParam = new SqlParameter("@ResponseMessage", SqlDbType.NVarChar, 256) { Direction = ParameterDirection.Output };

				await _context.Database.ExecuteSqlRawAsync(
				   "EXEC [dbo].[InsertApplication] @Name, @Contact, @Address, @Email, @CoverLetter, @Resume, @Jid, @ResponseMessage OUTPUT",
				   NameParam, ContactParam, AddressParam, EmailParam, CLParam, ResumeParam, jidParam, responseMessageParam
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
