using myapi.DTOs;
using myapi.Model;

namespace myapi.Repositories
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetEmployeeByIdAsync(int id);

        //Task<List<EmployeeResponseDto>> GetAllEmployeesAsync();
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();


        Task<Employee> CreateAsync(Employee employee);

        Task<bool> UpdateAsync(Employee employee);

        Task<bool> DeleteAsync(int id);


    }
}
