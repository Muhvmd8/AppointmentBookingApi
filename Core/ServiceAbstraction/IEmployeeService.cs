using Shared;
using Shared.DTOs.Employee;

namespace ServiceAbstraction
{
    public interface IEmployeeService
    {
        Task<Result<EmployeeResponse>> CreateAsync(CreateEmployeeRequest request);

        Task<Result<EmployeeResponse>> GetByIdAsync(int id);

        Task<Result<PagedResult<EmployeeResponse>>> GetAllAsync(int pageNumber, int pageSize);

        Task<Result<EmployeeResponse>> UpdateAsync(int id, UpdateEmployeeRequest request);

        Task<Result> DeactivateAsync(int id);
    }
}
