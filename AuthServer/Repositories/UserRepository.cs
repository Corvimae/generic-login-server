using AuthServer.Interfaces;
using AuthServer.Models;
using AuthServer.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Repositories {
	public class UserRepository : IUserRepository {
		private readonly ApplicationDbContext dbContext;

		public UserRepository(ApplicationDbContext dbContext) {
			this.dbContext = dbContext;
		}

		public User GetUserById(long id) {
			return dbContext.Users.Find(id);
		}

		public async Task<User> AuthenticateAsync(string email, string password) {
			if(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) {
				return null;
			}

			User user = await dbContext.Users.SingleOrDefaultAsync(x => x.Email == email);

			if(user == null || !VerifyPassword(password, user.PasswordHash, user.PasswordSalt)) {
				return null;
			}

			return user;
		}

		public async Task<User> CreateUserAsync(User user, string password) {
			if(string.IsNullOrWhiteSpace(password)) {
				throw new RegistrationException(new { password = "Please enter a password." });
			}

			if(string.IsNullOrWhiteSpace(user.Username)) {
				throw new RegistrationException(new { username = "Please enter a username." });
			}

			if(string.IsNullOrWhiteSpace(user.Email)) {
				throw new RegistrationException(new { email = "Please enter your email." });
			}

			if(dbContext.Users.Any(x => x.Username == user.Username)) {
				throw new RegistrationException(new { username = "This username is already taken." });
			}

			if(dbContext.Users.Any(x => x.Email == user.Email)) {
				throw new RegistrationException(new { email = "There is already an account registered with this email address." });
			}

			byte[] passwordHash, passwordSalt;
			CreatePassword(password, out passwordHash, out passwordSalt);

			user.PasswordHash = passwordHash;
			user.PasswordSalt = passwordSalt;

			await dbContext.Users.AddAsync(user);
			await dbContext.SaveChangesAsync();

			return user;
		}

		public async Task DeleteUserAsync(long id) {
			User user = await dbContext.Users.FindAsync(id);

			if(user == null) throw new ServiceException("No user with the ID " + id + "exists.");

			dbContext.Users.Remove(user);

			await dbContext.SaveChangesAsync();
		}

		private void CreatePassword(string password, out byte[] passwordHash, out byte[] passwordSalt) {
			if(string.IsNullOrWhiteSpace(password)) {
				throw new ArgumentException("Password cannot be empty or just whitespace.", "password");
			}

			using(HMACSHA512 hmac = new HMACSHA512()) {
				passwordSalt = hmac.Key;
				passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
			}
		}

		private bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt) {
			if(string.IsNullOrWhiteSpace(password)) {
				throw new ArgumentException("Password cannot be empty or just whitespace.", "password");
			}

			if(storedHash.Length != 64) {
				throw new ArgumentException("Password hash length is incorrect; expected 64 bytes.", "storedHash");
			}

			if(storedSalt.Length != 128) {
				throw new ArgumentException("Password salt length is incorrect; expected 128 bytes.", "storedSalt");
			}

			using(HMACSHA512 hmac = new HMACSHA512(storedSalt)) {
				byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

				for(int i = 0; i < computedHash.Length; i += 1) {
					if(computedHash[i] != storedHash[i]) return false;
				}
			}

			return true;
		}
	}
}
