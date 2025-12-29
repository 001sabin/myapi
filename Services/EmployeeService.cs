using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using myapi.Data;
using myapi.DTOs;
using myapi.Model;

namespace myapi.Services
{
    public class EmployeeService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public EmployeeService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<EmployeeResponseDto>> GetEmployeesAsync()
        {
            var employees = await _context.Employees.ToListAsync();
            return _mapper.Map<List<EmployeeResponseDto>>(employees);
        }

        public async Task<EmployeeResponseDto?> GetEmployeeByIdAsync(int id)
        {
            var employee=  await _context.Employees.FindAsync(id);
            return employee == null ? null : _mapper.Map<EmployeeResponseDto>(employee);
        }

        public async Task<EmployeeResponseDto> CreateEmployeeAsync(CreateEmployeeDto createDto)
        {

            var employee = _mapper.Map<Employee>(createDto);
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return _mapper.Map<EmployeeResponseDto>(employee);
        }

        public async Task<bool> UpdateEmployeeAsync(int id,  UpdateEmployeeDto updateDto)
        {
           // if (id != updatedEmployee.Id) return false;

            var existing = await _context.Employees.FindAsync(id);
            if (existing == null) return false;

            //existing.Name = updatedEmployee.Name;
            //existing.Position = updatedEmployee.Position;
            //existing.Department = updatedEmployee.Department;
            //existing.Salary = updatedEmployee.Salary;

            _mapper.Map(updateDto, existing);

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

        //// Patch applying + saving (ModelState validation should stay in controller)
        //public async Task<bool> ApplyPatchAndSaveAsync(int id, JsonPatchDocument<Employee> patchDocument)
        //{
        //    var employee = await _context.Employees.FindAsync(id);
        //    if (employee == null) return false;


        //    //patch vaneko = replace add or remove
        //    patchDocument.ApplyTo(employee);

        //    await _context.SaveChangesAsync();
        //    return true;
        //}

      
        public async Task<bool> PatchEmployeeAsync(int id, JsonPatchDocument<UpdateEmployeeDto> patchDocument)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return false;

            // Convert entity to DTO, apply patch, then map back
            var employeeDto = _mapper.Map<UpdateEmployeeDto>(employee);
            patchDocument.ApplyTo(employeeDto);
            _mapper.Map(employeeDto, employee);

            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<List<EmployeeResponseDto>> SearchEmployeesAsync(EmployeeSearchDto searchDto)
        {
            // Step 1: Start with IQueryable - NO database hit yet!
            IQueryable<Employee> query = _context.Employees.AsQueryable();

            // Step 2: Add filters dynamically - STILL no database hit!

            // Filter by department
            if (!string.IsNullOrEmpty(searchDto.Department))
            {
                query = query.Where(e => e.Department == searchDto.Department);
            }

            // Filter by minimum salary
            if (searchDto.MinSalary.HasValue)
            {
                query = query.Where(e => e.Salary >= searchDto.MinSalary.Value);
            }

            // Filter by maximum salary
            if (searchDto.MaxSalary.HasValue)
            {
                query = query.Where(e => e.Salary <= searchDto.MaxSalary.Value);
            }

            // Search in name (partial match)
            if (!string.IsNullOrEmpty(searchDto.NameContains))
            {
                query = query.Where(e => e.Name.Contains(searchDto.NameContains));
            }

            // Search in position (partial match)
            if (!string.IsNullOrEmpty(searchDto.PositionContains))
            {
                query = query.Where(e => e.Position.Contains(searchDto.PositionContains));
            }

            // Step 3: Apply paging - STILL no database hit!
            query = query
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize);

            // Step 4: Execute the query - ONE database hit with ALL filters!
            var employees = await query.ToListAsync();

            // Step 5: Map to DTO and return
            return _mapper.Map<List<EmployeeResponseDto>>(employees);
        }

        //// Bonus: Simple method to get high salary employees
        //public async Task<List<EmployeeResponseDto>> GetHighSalaryEmployeesAsync(decimal minSalary = 1000)
        //{
        //    var query = _context.Employees
        //        .Where(e => e.Salary > minSalary)
        //        .OrderByDescending(e => e.Salary);

        //    var employees = await query.ToListAsync();
        //    return _mapper.Map<List<EmployeeResponseDto>>(employees);
        //}
    }
}
