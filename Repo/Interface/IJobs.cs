using GTL.Models;

namespace GTL.Repo.Interface
{
	public interface IJobs
	{
		Task<string> AddJobAsync(Job job);
	}
}
