using System.ComponentModel.DataAnnotations;

namespace myapi.DTOs
{
    public class UpdateEmployeeDto
    {

        [Required]
        public string Name { get; set; }

        [Required]
        public string Position { get; set; }

        [Required]
        public string Department { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Salary { get; set; }
    }
}
