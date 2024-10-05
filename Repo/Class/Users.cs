using GTL.Models;
using GTL.Repo.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace GTL.Repo.Class
{
	public class Users : IUser
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<Users> _logger;

		public Users(ApplicationDbContext context, ILogger<Users> logger)
		{
			_context = context;
			_logger = logger;
		}


		public async Task<string> UpdateUserAsync(string email, string password, int? id)
		{
			try
			{
				var emailParam = new SqlParameter("@Email", email ?? (object)DBNull.Value);
				var passwordParam = new SqlParameter("@Password", password ?? (object)DBNull.Value);
				var idParam = new SqlParameter("@Id", id.HasValue ? (object)id.Value : DBNull.Value);

				var responseMessageParam = new SqlParameter("@ResponseMessage", SqlDbType.NVarChar, 256) { Direction = ParameterDirection.Output };

				await _context.Database.ExecuteSqlRawAsync(
				   "EXEC [dbo].[UpdateUser] @Email, @Password, @Id, @ResponseMessage OUTPUT",
				   emailParam, passwordParam, idParam, responseMessageParam
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
