using GTL.Models;

namespace GTL.Repo.Interface
{
	public interface IApplication
	{
		Task<string> AddApplicationAsync(Applications applications);
	}
}
