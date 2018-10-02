using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer.Utilities {
	public class RegistrationException : Exception {
		public object Issue { get; set; }

		public RegistrationException(object issue) : base() {
			Issue = issue;
		}
	}
}
