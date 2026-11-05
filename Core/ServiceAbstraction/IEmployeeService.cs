using Shared;
using Shared.DTOs.Employee;

namespace ServiceAbstraction
{
    public interface IEmployeeService
    {
        Task<Result<EmployeeResponse>> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default);

        Task<Result<EmployeeResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<Result<PagedResult<EmployeeResponse>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        Task<Result<EmployeeResponse>> UpdateAsync(int id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default);

        Task<Result> DeactivateAsync(int id, CancellationToken cancellationToken = default);
    }
}
