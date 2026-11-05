namespace Service
{
    using AutoMapper;
    using Domain.Contracts;
    using Domain.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using ServiceAbstraction;
    using Shared;
    using Shared.DTOs.Employee;
    using Shared.Errors;

    public sealed class EmployeeService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<EmployeeService> logger) : IEmployeeService
    {
        public async Task<Result<EmployeeResponse>> CreateAsync(
            CreateEmployeeRequest request,
            CancellationToken cancellationToken = default)
        {
            var employeeResult = Employee.Create(request.Name, request.PhoneNumber);

            if (employeeResult.IsFailure)
                return Result<EmployeeResponse>.Failure(employeeResult.Error!);

            var employee = employeeResult.Value;

            var repository = unitOfWork.GetRepository<Employee, int>();

            await repository.AddAsync(employee, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            var response = mapper.Map<EmployeeResponse>(employee);

            logger.LogInformation(
                "Created new employee with ID {Id}, Name {Name}, and Phone Number {PhoneNumber}.",
                employee.Id,
                employee.Name,
                employee.PhoneNumber);

            return Result<EmployeeResponse>.Success(response);
        }

        public async Task<Result<EmployeeResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var repository = unitOfWork.GetRepository<Employee, int>();

            var employee = await repository.GetFirstOrDefaultAsync(
                x => x.Id == id, cancellationToken);

            if (employee is null)
                return Result<EmployeeResponse>.Failure(EmployeeErrors.NotFound);

            var response = mapper.Map<EmployeeResponse>(employee);

            return Result<EmployeeResponse>.Success(response);
        }

        public async Task<Result<PagedResult<EmployeeResponse>>> GetAllAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber <= 0 || pageSize <= 0 || pageSize > 100)
                return Result<PagedResult<EmployeeResponse>>.Failure(EmployeeErrors.InvalidPagination);

            var repository = unitOfWork.GetRepository<Employee, int>();

            var query = repository
                .GetAllQueryable()
                .AsNoTracking()
                .OrderBy(x => x.Name);

            var totalCount = await query.CountAsync(cancellationToken);

            var employees = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var mappedEmployees = mapper.Map<List<EmployeeResponse>>(employees);

            var pagedResult = new PagedResult<EmployeeResponse>(
                mappedEmployees,
                totalCount);

            return Result<PagedResult<EmployeeResponse>>.Success(pagedResult);
        }

        public async Task<Result<EmployeeResponse>> UpdateAsync(int id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default)
        {
            var repository = unitOfWork.GetRepository<Employee, int>();

            var employee = await repository.GetFirstOrDefaultAsync(
                x => x.Id == id, cancellationToken);

            if (employee is null)
                return Result<EmployeeResponse>.Failure(EmployeeErrors.NotFound);

            var updateResult = employee.UpdateInformation(request.Name, request.PhoneNumber);

            if (updateResult.IsFailure)
                return Result<EmployeeResponse>.Failure(updateResult.Error!);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            var response = mapper.Map<EmployeeResponse>(employee);

            logger.LogInformation(
                "Updated employee with ID {Id}. New Name: {Name}, New Phone Number: {PhoneNumber}.",
                employee.Id,
                employee.Name,
                employee.PhoneNumber);

            return Result<EmployeeResponse>.Success(response);
        }

        public async Task<Result> DeactivateAsync(int id, CancellationToken cancellationToken = default)
        {
            var repository = unitOfWork.GetRepository<Employee, int>();

            var employee = await repository.GetFirstOrDefaultAsync(
                x => x.Id == id, cancellationToken);

            if (employee is null)
                return Result.Failure(EmployeeErrors.NotFound);

            if (!employee.IsActive)
                return Result.Failure(EmployeeErrors.Inactive);

            employee.Deactivate();

            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Deactivated employee with ID {Id}.",
                employee.Id);

            return Result.Success();
        }
    }
}
