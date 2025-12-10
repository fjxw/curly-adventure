using HRManagement.Employees.Api.Domain.Entities;
using HRManagement.Employees.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Employees.Api.Infrastructure.Repositories;

public interface IDepartmentRepository : IRepository<Department>
{
    Task<Department?> GetByIdWithEmployeesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Department>> GetTopLevelDepartmentsAsync(CancellationToken cancellationToken = default);
}

public class DepartmentRepository : Repository<Department>, IDepartmentRepository
{
    public DepartmentRepository(EmployeesDbContext context) : base(context)
    {
    }

    public async Task<Department?> GetByIdWithEmployeesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Employees)
            .Include(d => d.Positions)
            .Include(d => d.ChildDepartments)
            .AsSplitQuery()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Department>> GetTopLevelDepartmentsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.ChildDepartments)
            .Where(d => d.ParentDepartmentId == null)
            .ToListAsync(cancellationToken);
    }
}
