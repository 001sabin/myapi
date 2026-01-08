using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using myapi.DTOs;
using myapi.ExtensionMethods;
using myapi.Filter;
using myapi.Model;
using myapi.Services;
//using Serilog;

namespace myapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly EmployeeService _service;
        private readonly ILogger<EmployeesController> _logger;

        // private readonly IMapper _mapper;

        public EmployeesController(EmployeeService service, ILogger<EmployeesController> logger)
        {
            _service = service;
            _logger = logger;
            //  _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetEmployees()
        {
                        _logger.LogInformation("Fetching all employees from the database {method}.",HttpContext.Request.Method);

            var employees = await _service.GetEmployeesAsync();
            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeResponseDto>> GetEmployee(int id)
        {
           
            var employee = await _service.GetEmployeeByIdAsync(id);

            if (employee == null)
            {
                _logger.LogWarning("Employee with ID: {EmployeeId} was not found", id);
                return NotFound("Employee not found");
            }
            return Ok(employee);
        }

        [HttpPost]
        public async Task<ActionResult<EmployeeResponseDto>> CreateEmployee(CreateEmployeeDto createDto)
        {
            //if (!createDto.Name.IsValidEmployeeName())
            //    return BadRequest("Invalid employee name");

            var created = await _service.CreateEmployeeAsync(createDto);
            _logger.LogInformation("User is creating employee {@Employee}", createDto);
            return CreatedAtAction(nameof(GetEmployee), new { id = created.Id }, created);
        }

        [ServiceFilter(typeof(PutLoggingActionFilter))]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, UpdateEmployeeDto updateDto)
        {
            //if (id != updatedEmployee.Id)
            //    return BadRequest("ID mismatch");

            var success = await _service.UpdateEmployeeAsync(id, updateDto);

            if (!success)
            {
                _logger.LogInformation("User is updating employee {@Employee}", updateDto);
                return NotFound("Employee not found");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var success = await _service.DeleteEmployeeAsync(id);

            if (!success)
                return NotFound("Employee not found");// HTTP 404 -> not found

            return NoContent();// HTTP 204 -> successful with no return content
        }

        //[HttpPatch("{id}")]
        //public async Task<IActionResult> PatchEmployee(int id, JsonPatchDocument<Employee> patchDocument)
        //{
        //    if (patchDocument == null)
        //        return BadRequest("Patch document is null");

        //    // For PATCH, keep ModelState validation here (HTTP layer responsibility)
        //    var employee = await _service.GetEmployeeByIdAsync(id);
        //    if (employee == null)
        //        return NotFound("Employee not found");

        //    patchDocument.ApplyTo(employee, ModelState);

        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    // Save using service (reuse update method)
        //    //var saved = await _service.UpdateEmployeeAsync(id, employee);
        //    var saved = await _service.ApplyPatchAndSaveAsync(id, patchDocument);

        //    if (!saved)
        //        return NotFound("Employee not found");

        //    return NoContent();
        //}

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchEmployee(int id, JsonPatchDocument<UpdateEmployeeDto> patchDocument)
        {
            if (patchDocument == null)
                return BadRequest("Patch document is null");

            // Validate the patch document
            var updateDto = new UpdateEmployeeDto();
            patchDocument.ApplyTo(updateDto, ModelState);

            if (!TryValidateModel(updateDto))
                return BadRequest(ModelState);

            var success = await _service.PatchEmployeeAsync(id, patchDocument);

            if (!success)
                return NotFound("Employee not found");

            return NoContent();
        }



        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> SearchEmployees(
    [FromQuery] EmployeeSearchDto searchDto)
        {
            // Validate page size (optional)
            if (searchDto.PageSize > 100)
            {
                return BadRequest("Page size cannot exceed 100");
            }

            if (searchDto.Page < 1)
            {
                return BadRequest("Page must be at least 1");
            }

            var employees = await _service.SearchEmployeesAsync(searchDto);

            if (employees == null || employees.Count == 0)
            {
                return NotFound("No employees found matching your criteria");
            }

            return Ok(employees);
        }

        //// Simple high salary endpoint
        //[HttpGet("high-salary")]
        //public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetHighSalaryEmployees(
        //    [FromQuery] decimal minSalary = 1000)
        //{
        //    var employees = await _service.GetHighSalaryEmployeesAsync(minSalary);
        //    return Ok(employees);
        //}
    }
}
