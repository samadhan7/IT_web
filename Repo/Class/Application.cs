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


				var responseMessageParam = new SqlParameter("@ResponseMessage", SqlDbType.NVarChar, 256) { Direction = ParameterDirection.Output };

				await _context.Database.ExecuteSqlRawAsync(
				   "EXEC [dbo].[InsertApplication] @Name, @Contact, @Address, @Email, @CoverLetter, @Resume, @ResponseMessage OUTPUT",
				   NameParam, ContactParam, AddressParam, EmailParam, CLParam, ResumeParam, responseMessageParam
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
