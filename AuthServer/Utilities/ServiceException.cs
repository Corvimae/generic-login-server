using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer.Utilities {
	public class ServiceException : Exception {
		public ServiceException(string message) : base(message) { }
	}
}
