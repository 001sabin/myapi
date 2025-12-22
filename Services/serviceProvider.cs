using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using myapi.Data;
using myapi.Model;

namespace myapi.Services
{
    public class EmployeeService
    {
        private readonly AppDbContext _context;

        public EmployeeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Employee>> GetEmployeesAsync()
        {
            return await _context.Employees.ToListAsync();
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            return await _context.Employees.FindAsync(id);
        }

        public async Task<Employee> CreateEmployeeAsync(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task<bool> UpdateEmployeeAsync(int id, Employee updatedEmployee)
        {
            if (id != updatedEmployee.Id) return false;

            var existing = await _context.Employees.FindAsync(id);
            if (existing == null) return false;

            existing.Name = updatedEmployee.Name;
            existing.Position = updatedEmployee.Position;
            existing.Department = updatedEmployee.Department;
            existing.Salary = updatedEmployee.Salary;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return false;

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return true;
        }

        // Patch applying + saving (ModelState validation should stay in controller)
        public async Task<bool> ApplyPatchAndSaveAsync(int id, JsonPatchDocument<Employee> patchDocument)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return false;


            //patch vaneko = replace add or remove
            patchDocument.ApplyTo(employee);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
