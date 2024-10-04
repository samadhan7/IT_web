using GTL.Models;

namespace GTL.Repo.Interface
{
	public interface IInquiry
	{
		Task<string> AddInquiryAsync(Inquiry inquiry);
	}
}
