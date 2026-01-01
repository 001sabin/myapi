using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using myapi.DTOs;
using Dapper;
using myapi.Model;

namespace myapi.Repositories
{
    public class DapperRepository : IEmployeeRepository
    {
        private readonly string _connectionString;
        public DapperRepository(IConfiguration configuration)
        {
            //_connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration), "Connectio string not found vetena hai maile ta");
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            using var connection = CreateConnection();
            var query = "SELECT Id, Name, Position, Department FROM Employees WHERE Id = @Id";
            //var employee = await connection.QuerySingleOrDefaultAsync<EmployeeResponseDto>(query, new { Id = id });
            //return employee;
            return await connection.QuerySingleOrDefaultAsync<Employee>(query, new { Id = id });

        }
        //public async Task<List<EmployeeResponseDto>> GetAllEmployeesAsync()
        //{
        //    using var connection = CreateConnection();
        //    var query = "Select * from Employees ";
        //    var employees = await connection.QueryAsync<EmployeeResponseDto>(query);
        //    return employees.AsList();
        //}

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            using var connection = CreateConnection();
            var query = "Select * from Employees ";
           return await connection.QueryAsync<Employee>(query);
           
        }

        public async Task<Employee> CreateAsync(Employee employee)
        {
            using var connection = CreateConnection();
            var query = "INSERT INTO Employees (Name, Position, Department, Salary) VALUES (@Name, @Position, @Department, @Salary); " +
                        "SELECT CAST(SCOPE_IDENTITY() as int)";
            var id = await connection.QuerySingleAsync<int>(query, employee);
            employee.Id = id;
            return employee;
        }

        public async Task<bool> UpdateAsync(Employee employee)
        {
            using var connection = CreateConnection();
            var query = "UPDATE Employees SET Name = @Name, Position = @Position, Department = @Department, Salary = @Salary WHERE Id = @Id";
            var affectedRows = await connection.ExecuteAsync(query, employee);
            return affectedRows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = CreateConnection();
            var query = "DELETE FROM Employees WHERE Id = @Id";
            var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
            return affectedRows > 0;
        }


    }
}
