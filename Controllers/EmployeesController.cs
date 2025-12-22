using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using myapi.Filter;
using myapi.Model;
using myapi.Services;

namespace myapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly EmployeeService _service;

        public EmployeesController(EmployeeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            var employees = await _service.GetEmployeesAsync();
            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            var employee = await _service.GetEmployeeByIdAsync(id);

            if (employee == null)
                return NotFound("Employee not found");

            return Ok(employee);
        }

        [HttpPost]
        public async Task<ActionResult<Employee>> CreateEmployee(Employee employee)
        {
            var created = await _service.CreateEmployeeAsync(employee);
            return CreatedAtAction(nameof(GetEmployee), new { id = created.Id }, created);
        }

        [ServiceFilter(typeof(PutLoggingActionFilter))]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, Employee updatedEmployee)
        {
            if (id != updatedEmployee.Id)
                return BadRequest("ID mismatch");

            var success = await _service.UpdateEmployeeAsync(id, updatedEmployee);

            if (!success)
                return NotFound("Employee not found");

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

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchEmployee(int id, JsonPatchDocument<Employee> patchDocument)
        {
            if (patchDocument == null)
                return BadRequest("Patch document is null");

            // For PATCH, keep ModelState validation here (HTTP layer responsibility)
            var employee = await _service.GetEmployeeByIdAsync(id);
            if (employee == null)
                return NotFound("Employee not found");

            patchDocument.ApplyTo(employee, ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Save using service (reuse update method)
            //var saved = await _service.UpdateEmployeeAsync(id, employee);
            var saved = await _service.ApplyPatchAndSaveAsync(id, patchDocument);

            if (!saved)
                return NotFound("Employee not found");

            return NoContent();
        }
    }
}
