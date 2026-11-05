using Microsoft.AspNetCore.Http;
using Presentation.Factories;
using ServiceAbstraction;
using Shared;
using Shared.DTOs.Employee;
using Shared.Models;

namespace Presentation.Controllers
{
    public sealed class EmployeesController(
        IEmployeeService employeeService) : ApiController
    {
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EmployeeResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<EmployeeResponse>>> CreateEmployee(
            [FromBody] CreateEmployeeRequest request)
        {
            var result = await employeeService.CreateAsync(request);

            if (result.IsFailure)
                return ApiResponseFactory.Problem(result, HttpContext.TraceIdentifier);

            return Ok(
                ApiResponseFactory.Success(
                    result.Value,
                    HttpContext.TraceIdentifier,
                    "Employee created successfully."));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<EmployeeResponse>>> GetEmployeeById(int id)
        {
            var result = await employeeService.GetByIdAsync(id);

            if (result.IsFailure)
                return ApiResponseFactory.Problem(result, HttpContext.TraceIdentifier);

            return Ok(
                ApiResponseFactory.Success(
                    result.Value,
                    HttpContext.TraceIdentifier,
                    "Employee retrieved successfully."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<EmployeeResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResult<EmployeeResponse>>>> GetEmployees(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await employeeService.GetAllAsync(pageNumber, pageSize);

            if (result.IsFailure)
                return ApiResponseFactory.Problem(result, HttpContext.TraceIdentifier);

            return Ok(
                ApiResponseFactory.Success(
                    result.Value,
                    HttpContext.TraceIdentifier,
                    "Employees retrieved successfully.",
                    new PaginationMeta(pageNumber, pageSize, result.Value.TotalCount)));
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<EmployeeResponse>>> UpdateEmployee(
            int id,
            [FromBody] UpdateEmployeeRequest request)
        {
            var result = await employeeService.UpdateAsync(id, request);

            if (result.IsFailure)
                return ApiResponseFactory.Problem(result, HttpContext.TraceIdentifier);

            return Ok(
                ApiResponseFactory.Success(
                    result.Value,
                    HttpContext.TraceIdentifier,
                    "Employee updated successfully."));
        }

        [HttpPatch("{id:int}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<bool>>> DeactivateEmployee(int id)
        {
            var result = await employeeService.DeactivateAsync(id);

            if (result.IsFailure)
                return ApiResponseFactory.Problem(result, HttpContext.TraceIdentifier);

            return Ok(
                ApiResponseFactory.Success(
                    HttpContext.TraceIdentifier,
                    "Employee deactivated successfully."));
        }
    }
}
