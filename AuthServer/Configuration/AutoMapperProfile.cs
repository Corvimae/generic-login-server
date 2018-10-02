using AuthServer.Dtos;
using AuthServer.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer.Configuration {
	public class AutoMapperProfile : Profile {
		public AutoMapperProfile() {
			CreateMap<User, PublicUserDto>();
			CreateMap<UserAuthenticationDto, User>();
		}
	}
}
