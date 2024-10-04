using GTL.Models;

namespace GTL.Repo.Interface
{
	public interface IApplication
	{
		Task<string> AddApplicationAsync(Applications applications);

		Task<bool> CheckApplicationAsync(string email, string phone);

		Task<IEnumerable<Applications>> GetApplicationsAsync();
		Task<string> DeleteApplicationAsync(int id);
	}
}
