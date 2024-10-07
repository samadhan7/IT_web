using GTL.Models;
using GTL.Repo.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace GTL.Repo.Class
{
	public class Inquiries : IInquiry
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<Inquiries> _logger;

		public Inquiries(ApplicationDbContext context, ILogger<Inquiries> logger)
		{
			_context = context;
			_logger = logger;
		}


		public async Task<string> AddInquiryAsync(Inquiry inquiry)
		{
			try
			{
				var responseMessageParam = new SqlParameter("@ResponseMessage", SqlDbType.NVarChar, 255)
				{
					Direction = ParameterDirection.Output
				};

				var nameParam = new SqlParameter("@Name", inquiry.name ?? (object)DBNull.Value);
				var emailParam = new SqlParameter("@Email", inquiry.email ?? (object)DBNull.Value);
				var contactParam = new SqlParameter("@Contact", inquiry.contact ?? (object)DBNull.Value);
				var subjectParam = new SqlParameter("@Subject", inquiry.subject ?? (object)DBNull.Value);
				var messageParam = new SqlParameter("@Message", inquiry.message ?? (object)DBNull.Value);

				await _context.Database.ExecuteSqlRawAsync(
				  "EXEC [dbo].[InsertInquiry] @Name, @Email, @Contact, @Subject, @Message, @ResponseMessage OUTPUT",
				  nameParam,
				  emailParam,
				  contactParam,
				  subjectParam,
				  messageParam,
				  responseMessageParam);

				var ResponseMessage = responseMessageParam.Value == DBNull.Value ? string.Empty : (string)responseMessageParam.Value;

				return ResponseMessage;

			}
			catch (Exception ex)
			{

				return "Internal Server Error :" + ex.Message;
			}

		}

		public async Task<bool> DeleteInquiryAsync(int Id)
		{
			var successParam = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
			var IdParam = new SqlParameter("@Id", Id);

			try
			{
				await _context.Database.ExecuteSqlRawAsync("EXEC [dbo].[DeleteInquiry] @Id, @Success OUTPUT", IdParam, successParam);

				return (bool)successParam.Value;
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		public async Task<IEnumerable<Inquiry>> GetInquiriesAsync()
		{
			try
			{
				var InquiriesData = await _context.inquiries
											.FromSqlRaw("EXEC [dbo].[GetInquiries]")
											.ToListAsync();

				return InquiriesData;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while retrieving inquiries.");
				throw;
			}
		}

		public async Task<int> GetInquiriesCountAsync()
		{
			try
			{

				var countParam = new SqlParameter("@InquiryCount", SqlDbType.Int) { Direction = ParameterDirection.Output };

				await _context.Database.ExecuteSqlRawAsync("EXEC [CountInquiry] @InquiryCount OUTPUT", countParam);

				return (int)countParam.Value;

			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				throw;
			}
		}
	}
}
