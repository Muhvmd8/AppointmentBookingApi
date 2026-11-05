using AutoMapper;
using Domain.Models;
using Microsoft.Extensions.Options;
using Shared.DTOs.Appointment;
using Shared.DTOs.Employee;
using Shared.Settings;

namespace Service.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile() { }
        public MappingProfile(IOptions<AppSettings> appSettings)
        {
            var baseUrl = appSettings.Value.BaseUrl?.TrimEnd('/');

            #region Appointment
            CreateMap<Appointment, AppointmentResponse>();
            CreateMap<Appointment, EmployeeAppointmentResponse>();
            #endregion

            #region Employee
            CreateMap<Employee, EmployeeResponse>();
            #endregion
        }
    }
}