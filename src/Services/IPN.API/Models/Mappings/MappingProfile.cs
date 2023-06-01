using AutoMapper;

using Core.Domain.Entities;
using IPN.API.Models.DTOs.Responses;
using IPN.API.Models.DTOs.Requests;

namespace IPN.API.Models.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            ConfigureMappings();
        }

        private void ConfigureMappings()
        {
            CreateMap<Client, MinifiedClientDto>()
            .ForMember(destinationMember => destinationMember.ApiKey, options => options.MapFrom(src => src.ClientId));
            CreateMap<Client, ClientDto>();


        }

    }
   
}
