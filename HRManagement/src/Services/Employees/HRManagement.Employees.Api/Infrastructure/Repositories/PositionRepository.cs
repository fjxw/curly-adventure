using HRManagement.Employees.Api.Domain.Entities;
using HRManagement.Employees.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Employees.Api.Infrastructure.Repositories;

public interface IPositionRepository : IRepository<Position>
{
    Task<Position?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Position>> GetByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
}

public class PositionRepository : Repository<Position>, IPositionRepository
{
    public PositionRepository(EmployeesDbContext context) : base(context)
    {
    }

    public async Task<Position?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Department)
            .Include(p => p.Responsibilities)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Position>> GetByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Department)
            .Where(p => p.DepartmentId == departmentId)
            .ToListAsync(cancellationToken);
    }
}
