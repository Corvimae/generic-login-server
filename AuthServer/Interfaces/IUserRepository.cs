using AuthServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer.Interfaces {
	public interface IUserRepository {
		User GetUserById(long id);

		Task<User> AuthenticateAsync(string email, string password);

		Task<User> CreateUserAsync(User user, string password);

		Task UpdateUserAsync(long id, UserProfile profile);

		Task DeleteUserAsync(long id);
	}
}
