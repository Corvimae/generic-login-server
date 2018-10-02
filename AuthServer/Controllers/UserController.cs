using AuthServer.Configuration;
using AuthServer.Dtos;
using AuthServer.Interfaces;
using AuthServer.Models;
using AuthServer.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Controllers {
	[Authorize]
	[ApiController]
	[Route("api/[controller]")]
	public class UserController : ControllerBase {
		private readonly IUserRepository userRepository;
		private readonly IMapper mapper;
		private readonly AppSettings appSettings;

		public UserController(IUserRepository userRepository, IMapper mapper, IOptions<AppSettings> appSettings) {
			this.userRepository = userRepository;
			this.mapper = mapper;
			this.appSettings = appSettings.Value;
		}

		[AllowAnonymous]
		[HttpPost("authenticate")]
		public async Task<IActionResult> AuthenticateAsync([FromBody]UserAuthenticationDto userDto) {
			User user = await userRepository.AuthenticateAsync(userDto.Email, userDto.Password);

			if(user == null) {
				return Unauthorized();
			}

			JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
			byte[] key = Encoding.ASCII.GetBytes(appSettings.JWTGenerationCode);
			SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor {
				Subject = new ClaimsIdentity(new Claim[] {
					new Claim(ClaimTypes.Name, user.Id.ToString())
				}),
				Expires = DateTime.UtcNow.AddDays(7),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};

			SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

			return Ok(new {
				Id = user.Id,
				Username = user.Username,
				Token = tokenHandler.WriteToken(token)
			});
		}

		[AllowAnonymous]
		[HttpPost("register")]
		public async Task<IActionResult> RegisterAsync([FromBody] UserAuthenticationDto userDto) {
			User user = mapper.Map<User>(userDto);

			try {
				await userRepository.CreateUserAsync(user, userDto.Password);
				return Ok();
			} catch(RegistrationException e) {
				return BadRequest(e.Issue);
			}
		}

		[HttpGet("{id}")]
		public IActionResult GetById(long id) {
			User user = userRepository.GetUserById(id);

			return Ok(mapper.Map<PublicUserDto>(user));
		}
	}
}
