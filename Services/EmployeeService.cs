using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using myapi.Controllers;
using myapi.Data;
using myapi.Delegates;
using myapi.DTOs;
using myapi.Model;


using myapi.Repositories;

namespace myapi.Services
{
    public class EmployeeService
    {
        
        private readonly IMapper _mapper;
        //public EmployeeCreatedHandler? OnEmployeeCreated;
        
        private readonly IEmployeeRepository _repository
            ;
        private readonly ILogger<EmployeeService> _logger;
        public EmployeeService( IMapper mapper, IEmployeeRepository repository,ILogger<EmployeeService> logger)
        {
          
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<EmployeeResponseDto>> GetEmployeesAsync()
        {
            _logger.LogInformation("Fetching all employees from the database(Service).");
            //var employees = await _context.Employees.ToListAsync();
            //return _mapper.Map<List<EmployeeResponseDto>>(employees);

            var employees = await _repository.GetAllEmployeesAsync();
            return _mapper.Map<List<EmployeeResponseDto>>(employees);
        }

        public async Task<EmployeeResponseDto?> GetEmployeeByIdAsync(int id)
        {
            //var employee=  await _context.Employees.FindAsync(id);
            //return employee == null ? null : _mapper.Map<EmployeeResponseDto>(employee);
            _logger.LogInformation("Fetching employee with ID: {EmployeeId} from the database.", id);
            var employee = await _repository.GetEmployeeByIdAsync(id);
            return employee == null ? null : _mapper.Map<EmployeeResponseDto>(employee);
        }

        public async Task<EmployeeResponseDto> CreateEmployeeAsync(CreateEmployeeDto createDto)
        {

            var employee = _mapper.Map<Employee>(createDto);
            // _context.Employees.Add(employee);
            //await _context.SaveChangesAsync();
            //OnEmployeeCreated?.Invoke(employee);

            var createdEmployee = await _repository.CreateEmployeeAsync(employee);

            return _mapper.Map<EmployeeResponseDto>(createdEmployee);
        }

        public async Task<bool> UpdateEmployeeAsync(int id,  UpdateEmployeeDto updateDto)
        {
           // if (id != updatedEmployee.Id) return false;

            var existing = await _repository.GetEmployeeByIdAsync(id);
            if (existing == null) return false;

            //existing.Name = updatedEmployee.Name;
            //existing.Position = updatedEmployee.Position;
            //existing.Department = updatedEmployee.Department;
            //existing.Salary = updatedEmployee.Salary;

            _mapper.Map(updateDto, existing);

           // await _context.SaveChangesAsync();
            //return true;

            return await _repository.UpdateEmployeeAsync(existing);
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            var employee =  await _repository.GetEmployeeByIdAsync(id);
            if (employee == null) return false;

            //_context.Employees.Remove(employee);
            //await _context.SaveChangesAsync();
            //return true;
            return await _repository.DeleteEmployeeAsync(id);
        }

      
        public async Task<bool> PatchEmployeeAsync(int id, JsonPatchDocument<UpdateEmployeeDto> patchDocument)
        {
            var employee = await _repository.GetEmployeeByIdAsync(id);
            if (employee == null) return false;

            // Convert entity to DTO, apply patch, then map back
            var employeeDto = _mapper.Map<UpdateEmployeeDto>(employee);
            patchDocument.ApplyTo(employeeDto);
            _mapper.Map(employeeDto, employee);

            //await _context.SaveChangesAsync();
            //return true;

            return await _repository.UpdateEmployeeAsync(employee);
        }

        public async Task<List<EmployeeResponseDto>> SearchEmployeesAsync(EmployeeSearchDto searchDto)
        {
            var employees = await _repository.SearchEmployeesAsync(
                searchDto.Department,
                searchDto.MinSalary,
                searchDto.MaxSalary,
                searchDto.NameContains,
                searchDto.PositionContains,
                searchDto.Page,
                searchDto.PageSize);

            return _mapper.Map<List<EmployeeResponseDto>>(employees);
        }



    }
}
