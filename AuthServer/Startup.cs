using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthServer.Interfaces;
using AuthServer.Models;
using AuthServer.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using AuthServer.Configuration;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.HttpOverrides;

namespace AuthServer {
	public class Startup {
		public Startup(IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services) {
			services.AddDbContext<ApplicationDbContext>(opt => opt.UseNpgsql(Configuration["Data:ApplicationDbContext:ConnectionString"]));
			services.AddAutoMapper();

			IConfiguration appSettingsConfiguration = Configuration.GetSection("AppSettings");
			services.Configure<AppSettings>(appSettingsConfiguration);

			AppSettings appSettings = appSettingsConfiguration.Get<AppSettings>();

			byte[] jwtKey = Encoding.ASCII.GetBytes(appSettings.JWTGenerationCode);

			services
				.AddAuthentication(x => {
					x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
					x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				})
				.AddJwtBearer(x => {
					x.Events = new JwtBearerEvents {
						OnTokenValidated = context => {
							IUserRepository userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();

							long userId = long.Parse(context.Principal.Identity.Name);
							User user = userRepository.GetUserById(userId);

							if(user == null) context.Fail("Unauthorized");

							return Task.CompletedTask;
						}
					};

					x.RequireHttpsMetadata = false;
					x.SaveToken = true;
					x.TokenValidationParameters = new TokenValidationParameters {
						ClockSkew = TimeSpan.FromMinutes(5),
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
						RequireExpirationTime = true,
						ValidateLifetime = true,
						ValidateAudience = true,
						ValidAudience = "api://default",
						ValidateIssuer = false // TODO IMPORTANT DO NOT LEAVE THIS AS IT IS
					};
				});


			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			services.AddScoped<IUserRepository, UserRepository>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
			if(env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			} else {
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseAuthentication();

			app.UseMvc();

			using(IServiceScope serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope()) {
				serviceScope.ServiceProvider.GetService<ApplicationDbContext>().Database.Migrate();
			}
		}
	}
}
