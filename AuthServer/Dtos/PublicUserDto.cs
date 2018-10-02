using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer.Dtos {
	public class PublicUserDto {
		public long Id { get; set; }
		public string DisplayName { get; set; }
	}
}
