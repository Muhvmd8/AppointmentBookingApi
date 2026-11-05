using Microsoft.AspNetCore.Http;
using Presentation.Factories;
using ServiceAbstraction;
using Shared.DTOs.Appointment;
using Shared.Models;

namespace Presentation.Controllers
{
    public sealed class AppointmentsController(
        IAppointmentService appointmentService) : ApiController
    {
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AppointmentResponse>>> CreateAppointment(
            [FromBody] CreateAppointmentRequest request)
        {
            var result = await appointmentService.CreateAppointmentAsync(request);

            if (result.IsFailure)
                return ApiResponseFactory.Problem(result, HttpContext.TraceIdentifier);

            return Ok(
                ApiResponseFactory.Success(
                    result.Value,
                    HttpContext.TraceIdentifier,
                    "Appointment created successfully."));
        }

        [HttpGet("{employeeId:int}/available-slots")]
        [ProducesResponseType(
            typeof(ApiResponse<IReadOnlyList<AvailableSlotResponse>>),
            StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<AvailableSlotResponse>>>> GetAvailableSlots(
            [FromRoute] int employeeId,
            [FromQuery] DateOnly date,
            [FromQuery] int durationInMinutes)
        {
            var result = await appointmentService.GetAvailableSlotsAsync(
                employeeId,
                date,
                durationInMinutes);

            if (result.IsFailure)
                return ApiResponseFactory.Problem(
                    result,
                    HttpContext.TraceIdentifier);

            return Ok(
                ApiResponseFactory.Success(
                    result.Value,
                    HttpContext.TraceIdentifier,
                    "Available slots retrieved successfully."));
        }

        [HttpGet("{employeeId:int}/appointments")]
        [ProducesResponseType(
            typeof(ApiResponse<IReadOnlyList<EmployeeAppointmentResponse>>),
            StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<EmployeeAppointmentResponse>>> GetEmployeeSchedule(
            [FromRoute] int employeeId,
            [FromQuery] DateOnly date,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {

            var result = await appointmentService.GetEmployeeScheduleAsync(
                employeeId,
                date,
                pageNumber,
                pageSize);

            if (result.IsFailure)
                return ApiResponseFactory.Problem(
                    result,
                    HttpContext.TraceIdentifier);

            var pagedResult = result.Value;

            var pagination = new PaginationMeta(pageNumber, pageSize, pagedResult.TotalCount);

            return Ok(
                ApiResponseFactory.Success(
                    pagedResult.Items,
                    HttpContext.TraceIdentifier,
                    "Employee appointments retrieved successfully.",
                    pagination));
        }

        [HttpPatch("{appointmentId:long}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<bool>>> CancelAppointment(
            [FromRoute] long appointmentId)
        {
            var result = await appointmentService.CancelAppointmentAsync(
                appointmentId);

            if (result.IsFailure)
                return ApiResponseFactory.Problem(
                    result,
                    HttpContext.TraceIdentifier);

            return Ok(
                ApiResponseFactory.Success(
                    true,
                    HttpContext.TraceIdentifier,
                    "Appointment cancelled successfully."));
        }
    }
}
