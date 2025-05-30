using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.Model;

namespace ProvidingFood2.Repository
{
	public interface IUserRepository
	{
		Task<int> AddBaseUserAsync(User user, string userTypeName);
		Task<LoginResult> Login(Login login);



	}
}
