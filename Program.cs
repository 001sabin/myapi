using Microsoft.EntityFrameworkCore;
using myapi.Data;
using myapi.Middleware;
using myapi.Filter;
using myapi.Services;
using myapi.Delegates;
using Microsoft.Extensions.DependencyInjection;
//using Newtonsoft.Json.Serialization;
//using Microsoft.AspNetCore.JsonPatch;

using myapi.Repositories;


var builder = WebApplication.CreateBuilder(args);

// we have registered the dbcontext this is DI
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Explicit delegate creation
void ConfigureDbContext(DbContextOptionsBuilder options)
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
}
Action<DbContextOptionsBuilder> myDelegate = ConfigureDbContext;
builder.Services.AddDbContext<AppDbContext>(myDelegate);


// Dapper through get y id implement garna lai 
builder.Services.AddScoped<IEmployeeRepository, DapperRepository>();


//automapper ko registration
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddScoped<EmployeeService>();


// Add services to the container.
builder.Services.AddScoped<PutLoggingActionFilter>();

//Imiddleware implemetation ko example ko lagi
builder.Services.AddScoped<iRequestLoggingMiddleware>();

builder.Services.AddControllers().AddNewtonsoftJson();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();







var app = builder.Build();

//iMiddleware wala LOgging ko lagi
//app.UseMiddleware<iRequestLoggingMiddleware>();


//app.UseMiddleware<RateLimitingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();


app.UseAuthorization();
//app.MapGet("/api/hello", () => "Hello World!");
app.MapControllers();


//using (var scope = app.Services.CreateScope())
//{
//    var employeeService = scope.ServiceProvider.GetRequiredService<EmployeeService>();

//    employeeService.OnEmployeeCreated += employee =>
//    {
//        Console.WriteLine($"[DELEGATE] Employee Created: {employee.Name}, {employee.Department}");
//    };
//}


app.Run();



//IDisposable 