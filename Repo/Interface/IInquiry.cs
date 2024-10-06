using GTL.Models;

namespace GTL.Repo.Interface
{
	public interface IInquiry
	{
		Task<string> AddInquiryAsync(Inquiry inquiry);
		Task<IEnumerable<Inquiry>> GetInquiriesAsync();
		Task<bool> DeleteInquiryAsync(int Id);

		Task<int> GetInquiriesCountAsync();
	}
}
