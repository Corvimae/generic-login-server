using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace AuthServer {
	public class Program {
		public static void Main(string[] args) {
			IConfigurationRoot config = new ConfigurationBuilder()
				.AddEnvironmentVariables("")
				.Build();

			string url = config["ASPNETCORE_URLS"] ?? "http://*:5000";
			string env = config["ASPNETCORE_ENVIRONMENT"] ?? "Development";

			CreateWebHostBuilder(args)
				.UseUrls(url)
				.UseEnvironment(env)
				.Build()
				.Run();
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>();
	}
}
