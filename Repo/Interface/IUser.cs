using GTL.Models;

namespace GTL.Repo.Interface
{
	public interface IUser
	{
		Task<string> UpdateUserAsync(string email, string password, int? id);
	}
}
