using AutoMapper;
using Microsoft.Extensions.Options;
using Shared.Settings;

namespace Service.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile() { }
        public MappingProfile(IOptions<AppSettings> appSettings)
        {
            var baseUrl = appSettings.Value.BaseUrl?.TrimEnd('/');
        }
    }
}