using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using myapi.DTOs;
using Dapper;
using myapi.Model;

namespace myapi.Repositories
{
    public class DapperEmployeeRepository : IEmployeeRepository
    {
        private readonly string _connectionString;
        public DapperEmployeeRepository(IConfiguration configuration)
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
            //var query = "SELECT Id, Name, Position, Department FROM Employees WHERE Id = @Id";
            //var employee = await connection.QuerySingleOrDefaultAsync<EmployeeResponseDto>(query, new { Id = id });
            //return employee;
            //return await connection.QuerySingleOrDefaultAsync<Employee>(query, new { Id = id });
            return await connection.QuerySingleOrDefaultAsync<Employee>("GetById", new {id },commandType: CommandType.StoredProcedure);

        }
        //public async Task<List<EmployeeResponseDto>> GetAllEmployeesAsync()
        //{
        //    using var connection = CreateConnection();
        //    var query = "Select * from Employees ";
        //    var employees = await connection.QueryAsync<EmployeeResponseDto>(query);
        //    return employees.AsList();
        //}

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {/*
            using var connection = CreateConnection();
            var query = "Select * from Employees ";
            return await connection.QueryAsync<Employee>(query);
            */
            using var connection = CreateConnection();
            return await connection.QueryAsync<Employee>("GetAllEmployee", commandType: CommandType.StoredProcedure);


        }

        //public async Task<Employee> CreateEmployeeAsync(Employee employee)
        //{
        //    using var connection = CreateConnection();
        //    var query = "INSERT INTO Employees (Name, Position, Department, Salary) VALUES (@Name, @Position, @Department, @Salary); " +
        //                "SELECT CAST(SCOPE_IDENTITY() as int)";
        //    var id = await connection.QuerySingleAsync<int>(query, employee);
        //    employee.Id = id;
        //    return employee;
        //}

        public async Task<Employee> CreateEmployeeAsync(Employee employee)
        {
            using var connection = CreateConnection();

            // Call stored procedure and get the ID
            //var parameters = new DynamicParameters();
            //parameters.Add("@Name", employee.Name);
            //parameters.Add("@Position", employee.Position);
            //parameters.Add("@Department", employee.Department);
            //parameters.Add("@Salary", employee.Salary);
           


            var id = await connection.QuerySingleAsync<int>(
                "sp_CreateEmployee",
                new
                {
                    name = employee.Name,
                    position = employee.Position,
                    department = employee.Department,
                    salary = employee.Salary
                },
                commandType: CommandType.StoredProcedure
            );

            employee.Id = id;
            return employee;
        }

        public async Task<bool> UpdateEmployeeAsync(Employee employee)
        {
            using var connection = CreateConnection();
            //var query = "UPDATE Employees SET Name = @Name, Position = @Position, Department = @Department, Salary = @Salary WHERE Id = @Id";
            //var affectedRows = await connection.ExecuteAsync(query, employee);
            //return affectedRows > 0;

            var affectedRows = await connection.ExecuteAsync(
                "updateEmployee",
                new
                {
                    id = employee.Id,
                    name = employee.Name,
                    position = employee.Position,
                    department = employee.Department,
                    salary = employee.Salary
                },
                commandType: CommandType.StoredProcedure
            );
            return affectedRows > 0;
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            using var connection = CreateConnection();
            //var query = "DELETE FROM Employees WHERE Id = @Id";
            //var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
            var affectedRows = await connection.ExecuteAsync(
                "DeleteEmployee",
                new { id },
                commandType: CommandType.StoredProcedure);
            return affectedRows > 0;
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
            using var connection = CreateConnection();

            // Build dynamic WHERE clause
            var whereClauses = new List<string>();
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(department))
            {
                whereClauses.Add("Department = @Department");
                parameters.Add("Department", department);
            }

            if (minSalary.HasValue)
            {
                whereClauses.Add("Salary >= @MinSalary");
                parameters.Add("MinSalary", minSalary.Value);
            }

            if (maxSalary.HasValue)
            {
                whereClauses.Add("Salary <= @MaxSalary");
                parameters.Add("MaxSalary", maxSalary.Value);
            }

            if (!string.IsNullOrEmpty(nameContains))
            {
                whereClauses.Add("Name LIKE @NameContains");
                parameters.Add("NameContains", $"%{nameContains}%");
            }

            if (!string.IsNullOrEmpty(positionContains))
            {
                whereClauses.Add("Position LIKE @PositionContains");
                parameters.Add("PositionContains", $"%{positionContains}%");
            }

            var whereClause = whereClauses.Any()
                ? "WHERE " + string.Join(" AND ", whereClauses)
                : string.Empty;

            var query = $@"
                SELECT * FROM Employees
                {whereClause}
                ORDER BY Id
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            parameters.Add("Offset", (page - 1) * pageSize);
            parameters.Add("PageSize", pageSize);

            return await connection.QueryAsync<Employee>(query, parameters);
        }


    }
}
