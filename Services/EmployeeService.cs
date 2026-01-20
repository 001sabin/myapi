using myapi.ExtensionMethods; // Import our new extensions
using Microsoft.Extensions.Caching.Distributed;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using myapi.DTOs;
using myapi.Model;
using myapi.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace myapi.Services
{
    public class EmployeeService
    {
        private readonly IMapper _mapper;
        private readonly IEmployeeRepository _repository;
        private readonly ILogger<EmployeeService> _logger;
        private readonly IDistributedCache _cache; // Changed from IMemoryCache
        //private readonly IMemoryCache _caches;
        private const string ALL_EMPLOYEES_KEY = "employees_all";
        private const string EMPLOYEE_BY_ID_KEY = "employee_";

        public EmployeeService(IMapper mapper, IEmployeeRepository repository, ILogger<EmployeeService> logger, IDistributedCache cache)
        {
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
            _cache = cache;
        }

        public async Task<List<EmployeeResponseDto>> GetEmployeesAsync()
        {
            // 1. Try to get from Redis
            var cachedEmployees = await _cache.GetRecordAsync<List<EmployeeResponseDto>>(ALL_EMPLOYEES_KEY);

            if (cachedEmployees != null)
            {
                _logger.LogInformation("Retrieved all employees from Redis.");
                return cachedEmployees;
            }

            // 2. If not in Redis, get from DB
            _logger.LogInformation("Redis cache miss. Fetching from DB.");
            var employees = await _repository.GetAllEmployeesAsync();
            var result = _mapper.Map<List<EmployeeResponseDto>>(employees);

            // 3. Store in Redis for 5 minutes
            await _cache.SetRecordAsync(ALL_EMPLOYEES_KEY, result, TimeSpan.FromMinutes(5));

            return result;
        }

        public async Task<EmployeeResponseDto?> GetEmployeeByIdAsync(int id)
        {
            //string cacheKey = $"{EMPLOYEE_BY_ID_KEY}{id}";

            //var cachedEmployee = await _cache.GetRecordAsync<EmployeeResponseDto>(cacheKey);
            //if (cachedEmployee != null)
            //{
            //    _logger.LogInformation("Employee {Id} found in Redis.", id);
            //    return cachedEmployee;
            //}

            var employee = await _repository.GetEmployeeByIdAsync(id);
            if (employee == null) return null;

            var result = _mapper.Map<EmployeeResponseDto>(employee);
           // await _cache.SetRecordAsync(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;
        }

        // --- WRITE OPERATIONS (Must clear cache to prevent stale data) ---

        public async Task<EmployeeResponseDto> CreateEmployeeAsync(CreateEmployeeDto createDto)
        {
            var employee = _mapper.Map<Employee>(createDto);
            var createdEmployee = await _repository.CreateEmployeeAsync(employee);

            // Invalidate the "All Employees" list because the list has changed
            await _cache.RemoveAsync(ALL_EMPLOYEES_KEY);

            return _mapper.Map<EmployeeResponseDto>(createdEmployee);
        }

        public async Task<bool> UpdateEmployeeAsync(int id, UpdateEmployeeDto updateDto)
        {
            var existing = await _repository.GetEmployeeByIdAsync(id);
            if (existing == null) return false;

            _mapper.Map(updateDto, existing);
            var success = await _repository.UpdateEmployeeAsync(existing);

            if (success)
            {
                // Invalidate both the list and the specific employee record
                await _cache.RemoveAsync(ALL_EMPLOYEES_KEY);
                await _cache.RemoveAsync($"{EMPLOYEE_BY_ID_KEY}{id}");
            }

            return success;
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            var success = await _repository.DeleteEmployeeAsync(id);
            if (success)
            {
                await _cache.RemoveAsync(ALL_EMPLOYEES_KEY);
                await _cache.RemoveAsync($"{EMPLOYEE_BY_ID_KEY}{id}");
            }
            return success;
        }

        public async Task<bool> PatchEmployeeAsync(int id, JsonPatchDocument<UpdateEmployeeDto> patchDocument)
        {
            var employee = await _repository.GetEmployeeByIdAsync(id);
            if (employee == null) return false;

            var employeeDto = _mapper.Map<UpdateEmployeeDto>(employee);
            patchDocument.ApplyTo(employeeDto);
            _mapper.Map(employeeDto, employee);

            var success = await _repository.UpdateEmployeeAsync(employee);
            if (success)
            {
                await _cache.RemoveAsync(ALL_EMPLOYEES_KEY);
                await _cache.RemoveAsync($"{EMPLOYEE_BY_ID_KEY}{id}");
            }
            return success;
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





















































//using AutoMapper;
//using Microsoft.AspNetCore.JsonPatch;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Caching.Distributed;
//using Microsoft.Extensions.Caching.Memory;
//using myapi.Controllers;
//using myapi.Data;
//using myapi.Delegates;
//using myapi.DTOs;
//using myapi.Model;


//using myapi.Repositories;

//namespace myapi.Services
//{
//    public class EmployeeService
//    {

//        private readonly IMapper _mapper;
//        //public EmployeeCreatedHandler? OnEmployeeCreated;

//        private readonly IEmployeeRepository _repository
//            ;
//        private readonly ILogger<EmployeeService> _logger;

//        //private readonly IMemoryCache _cache;
//        //private readonly MemoryCacheEntryOptions _cacheOptions;
//        //private readonly TimeSpan _defaultCacheDuration = TimeSpan.FromMinutes(7);
//        private readonly IDistributedCache _cache;

//        //cache key
//        private const string ALL_EMPLOYEES_KEY = "employees_all";

//        private const string EMPLOYEE_BY_ID_KEY = "employee_";

//        public EmployeeService( IMapper mapper, IEmployeeRepository repository,ILogger<EmployeeService> logger, IDistributedCache cache)
//        {

//            _mapper = mapper;
//            _repository = repository;
//            _logger = logger;
//            _cache = cache;

//            //_cacheOptions = new MemoryCacheEntryOptions()
//            //    .SetAbsoluteExpiration(_defaultCacheDuration)
//            //    .SetPriority(CacheItemPriority.Normal)
//            //    .SetSize(1); // Size is required when SizeLimit is set on MemoryCache
//        }

//        public async Task<List<EmployeeResponseDto>> GetEmployeesAsync()
//        {
//            var cachedEmployees
//        }

//        public async Task<EmployeeResponseDto?> GetEmployeeByIdAsync(int id)
//        {
//            _logger.LogInformation("Fetching employee with ID: {EmployeeId}.", id);

//            var cacheKey = $"{EMPLOYEE_BY_ID_KEY}{id}";

//            if (_cache.TryGetValue(cacheKey, out EmployeeResponseDto? cachedEmployee))
//            {
//                _logger.LogDebug("Returning employee ID {Id} from cache", id);
//                return cachedEmployee;
//            }

//            _logger.LogDebug("Cache miss for employee ID {Id}. Querying database...", id);

//            var employee = await _repository.GetEmployeeByIdAsync(id);
//            if (employee == null)
//            {
//                _logger.LogDebug("Employee ID {Id} not found in database", id);
//                return null;
//            }

//            var result = _mapper.Map<EmployeeResponseDto>(employee);

//            _cache.Set(cacheKey, result,
//                new MemoryCacheEntryOptions()
//                    .SetAbsoluteExpiration(_defaultCacheDuration));

//            return result;
//        }



//        public async Task<EmployeeResponseDto> CreateEmployeeAsync(CreateEmployeeDto createDto)
//        {

//            var employee = _mapper.Map<Employee>(createDto);
//            // _context.Employees.Add(employee);
//            //await _context.SaveChangesAsync();
//            //OnEmployeeCreated?.Invoke(employee);

//            var createdEmployee = await _repository.CreateEmployeeAsync(employee);

//            return _mapper.Map<EmployeeResponseDto>(createdEmployee);
//        }

//        public async Task<bool> UpdateEmployeeAsync(int id,  UpdateEmployeeDto updateDto)
//        {
//           // if (id != updatedEmployee.Id) return false;

//            var existing = await _repository.GetEmployeeByIdAsync(id);
//            if (existing == null) return false;

//            //existing.Name = updatedEmployee.Name;
//            //existing.Position = updatedEmployee.Position;
//            //existing.Department = updatedEmployee.Department;
//            //existing.Salary = updatedEmployee.Salary;

//            _mapper.Map(updateDto, existing);

//           // await _context.SaveChangesAsync();
//            //return true;

//            return await _repository.UpdateEmployeeAsync(existing);
//        }

//        public async Task<bool> DeleteEmployeeAsync(int id)
//        {
//            var employee =  await _repository.GetEmployeeByIdAsync(id);
//            if (employee == null) return false;

//            //_context.Employees.Remove(employee);
//            //await _context.SaveChangesAsync();
//            //return true;
//            return await _repository.DeleteEmployeeAsync(id);
//        }


//        public async Task<bool> PatchEmployeeAsync(int id, JsonPatchDocument<UpdateEmployeeDto> patchDocument)
//        {
//            var employee = await _repository.GetEmployeeByIdAsync(id);
//            if (employee == null) return false;

//            // Convert entity to DTO, apply patch, then map back
//            var employeeDto = _mapper.Map<UpdateEmployeeDto>(employee);
//            patchDocument.ApplyTo(employeeDto);
//            _mapper.Map(employeeDto, employee);

//            //await _context.SaveChangesAsync();
//            //return true;

//            return await _repository.UpdateEmployeeAsync(employee);
//        }

//public async Task<List<EmployeeResponseDto>> SearchEmployeesAsync(EmployeeSearchDto searchDto)
//{
//    var employees = await _repository.SearchEmployeesAsync(
//        searchDto.Department,
//        searchDto.MinSalary,
//        searchDto.MaxSalary,
//        searchDto.NameContains,
//        searchDto.PositionContains,
//        searchDto.Page,
//        searchDto.PageSize);

//    return _mapper.Map<List<EmployeeResponseDto>>(employees);
//}



//    }
//}
