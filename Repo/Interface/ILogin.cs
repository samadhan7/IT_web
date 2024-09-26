using GTL.Models;

namespace GTL.Repo.Interface
{
    public interface ILogin
    {
        Task<LoginResponse> LoginUserAsync(string email, string password);
    }
}
