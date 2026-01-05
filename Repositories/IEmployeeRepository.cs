using myapi.DTOs;
using myapi.Model;

namespace myapi.Repositories
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetEmployeeByIdAsync(int id);

        //Task<List<EmployeeResponseDto>> GetAllEmployeesAsync();
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();


        Task<Employee> CreateEmployeeAsync(Employee employee);

        Task<bool> UpdateEmployeeAsync(Employee employee);

        Task<bool> DeleteEmployeeAsync(int id);

        Task<IEnumerable<Employee>> SearchEmployeesAsync(
            string? department,
            decimal? minSalary,
            decimal? maxSalary,
            string? nameContains,
            string? positionContains,
            int page,
            int pageSize);
    


}
}
