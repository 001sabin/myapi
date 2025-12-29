namespace myapi.DTOs
{
    public class EmployeeSearchDto
    {
        public string? Department { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public string? NameContains { get; set; }
        public string? PositionContains { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}