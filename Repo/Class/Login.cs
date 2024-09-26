using GTL.Models;
using GTL.Repo.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace GTL.Repo.Class
{
    public class Login : ILogin
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<Login> _logger;

        public Login(ApplicationDbContext context, ILogger<Login> logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<LoginResponse> LoginUserAsync(string email, string password)
        {
            try
            {
                var emailParam = new SqlParameter("@Email", email ?? (object)DBNull.Value);
                var passwordParam = new SqlParameter("@Password", password ?? (object)DBNull.Value);  // Ensure password is hashed if needed

                var responseCodeParam = new SqlParameter("@ResponseCode", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var responseMessageParam = new SqlParameter("@ResponseMessage", SqlDbType.NVarChar, 256) { Direction = ParameterDirection.Output };
                var userIdOutputParam = new SqlParameter("@UserIdOutput", SqlDbType.Int) { Direction = ParameterDirection.Output }; // Change to Int
                var roleParam = new SqlParameter("@Role", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC [dbo].[LoginUser] @Email, @Password, @ResponseCode OUTPUT, @ResponseMessage OUTPUT, @UserIdOutput OUTPUT, @Role OUTPUT",
                    emailParam, passwordParam, responseCodeParam, responseMessageParam, userIdOutputParam, roleParam
                );

                return new LoginResponse
                {
                    ResponseCode = (int)(responseCodeParam.Value ?? 0),
                    ResponseMessage = responseMessageParam.Value == DBNull.Value ? string.Empty : (string)responseMessageParam.Value,
                    UserId = userIdOutputParam.Value == DBNull.Value ? (int?)null : (int)userIdOutputParam.Value,  // Cast to int?
                    Role = roleParam.Value == DBNull.Value ? string.Empty : (string)roleParam.Value
                };

            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    ResponseCode = 500,
                    ResponseMessage = "An internal server error occurred.",
                    UserId = null,
                    Role = null
                };
            }
        }





    }
}
