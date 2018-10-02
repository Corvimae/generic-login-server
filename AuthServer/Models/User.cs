using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer.Models {
	public class User : DatedEntity {
		[Key]
		public long Id { get; set; }

		[Required]
		[StringLength(64, MinimumLength = 6)]
		public string Username { get; set; }

		[Required]
		[EmailAddress]
		public string Email { get; set; }
		public byte[] PasswordHash { get; set; }
		public byte[] PasswordSalt { get; set; }
	}
}
