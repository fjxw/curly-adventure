using HRManagement.Employees.Api.Domain.Entities;
using HRManagement.Employees.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Employees.Api.Infrastructure.Repositories;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<Employee?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Employee>> GetByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Employee>> GetActiveEmployeesAsync(CancellationToken cancellationToken = default);
}

public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(EmployeesDbContext context) : base(context)
    {
    }

    public async Task<Employee?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Department)
            .Include(e => e.Position)
            .Include(e => e.Documents)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Position)
            .Where(e => e.DepartmentId == departmentId && e.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Department)
            .Include(e => e.Position)
            .Where(e => e.IsActive)
            .ToListAsync(cancellationToken);
    }
}
