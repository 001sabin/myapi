using Microsoft.EntityFrameworkCore;
using myapi.Data;
using myapi.Model;

namespace myapi.Repositories
{
    public class EfEmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _context;
        public EfEmployeeRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            return await _context.Employees.FindAsync(id);
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            return await _context.Employees.ToListAsync();
        }
        //public async Task<Employee> CreateEmployeeAsync(Employee employee)
        //{
        //    _context.Employees.Add(employee);
        //    await _context.SaveChangesAsync();
        //    return employee;
        //}


        public async Task<Employee> CreateEmployeeAsync(Employee employee)
        {
            // For output parameters, we need to use SqlParameter
            var idParam = new Microsoft.Data.SqlClient.SqlParameter("@Id", System.Data.SqlDbType.Int)
            {
                Direction = System.Data.ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_CreateEmployee @Name = {0}, @Position = {1}, @Department = {2}, @Salary = {3}, @Id = {4} OUTPUT",
                employee.Name, employee.Position, employee.Department, employee.Salary, idParam
            );

            employee.Id = (int)idParam.Value;
            return employee;
        }

        public async Task<bool> UpdateEmployeeAsync(Employee employee)
        {
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            var employee = await GetEmployeeByIdAsync(id);
            if (employee == null) return false;

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<IEnumerable<Employee>> SearchEmployeesAsync(
            string? department,
            decimal? minSalary,
            decimal? maxSalary,
            string? nameContains,
            string? positionContains,
            int page,
            int pageSize)
        {
            var query = _context.Employees.AsQueryable();
            if (!string.IsNullOrEmpty(department))
            {
                query = query.Where(e => e.Department == department);
            }
            if (minSalary.HasValue)
            {
                query = query.Where(e => e.Salary >= minSalary.Value);
            }
            if (maxSalary.HasValue)
            {
                query = query.Where(e => e.Salary <= maxSalary.Value);
            }
            if (!string.IsNullOrEmpty(nameContains))
            {
                query = query.Where(e => e.Name.Contains(nameContains));
            }
            if (!string.IsNullOrEmpty(positionContains))
            {
                query = query.Where(e => e.Position.Contains(positionContains));
            }
            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
