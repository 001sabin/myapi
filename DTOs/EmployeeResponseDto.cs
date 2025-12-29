using System.ComponentModel.DataAnnotations;

namespace myapi.DTOs
{
    public class EmployeeResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public decimal Salary { get; set; }
    }
}
