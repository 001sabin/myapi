using Microsoft.EntityFrameworkCore;
using myapi.Data;
using myapi.Middleware;
using myapi.Filter;
using myapi.Services;
using myapi.Delegates;
using Microsoft.Extensions.DependencyInjection;
//using Newtonsoft.Json.Serialization;
//using Microsoft.AspNetCore.JsonPatch;

using Serilog;

using myapi.Repositories;
using Serilog.Events;
using myapi.ExtensionMethods;


//var builder = WebApplication.CreateBuilder(args);
//Log.Logger = new LoggerConfiguration()
//  .ReadFrom.Configuration(builder.Configuration)
//  .CreateLogger();
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{

    Log.Information("Starting up the application");
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context,services,configuration)=>configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)//Diharu check garxa
    .Enrich.FromLogContext()
    );


    // we have registered the dbcontext this is DI
    //builder.Services.AddDbContext<AppDbContext>(options =>
    //    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddSqlServiceExtension(builder.Configuration);// this class is define as extension service in ExtensionMethods folder
    // Explicit delegate creation

    //void ConfigureDbContext(DbContextOptionsBuilder options)
    //{
    //    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    //}
    //Action<DbContextOptionsBuilder> myDelegate = ConfigureDbContext;
    //builder.Services.AddDbContext<AppDbContext>(myDelegate);
    builder.Services.AddRedisServiceExtension(builder.Configuration);

    // Dapper through get method implement garna lai 
    builder.Services.AddScoped<IEmployeeRepository, DapperEmployeeRepository>();
    //Ef through bata method implement garna lai 
    //builder.Services.AddScoped<IEmployeeRepository, EfEmployeeRepository>();

    //automapper ko registration
    builder.Services.AddAutoMapper(typeof(Program));

    builder.Services.AddScoped<EmployeeService>();


    // Add services to the container.
    builder.Services.AddScoped<PutLoggingActionFilter>();

    //Imiddleware implemetation ko example ko lagi
    builder.Services.AddScoped<iRequestLoggingMiddleware>();

    builder.Services.AddMemoryCache();

    builder.Services.AddControllers().AddNewtonsoftJson();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddResponseCaching();







    var app = builder.Build();
    //app.UseSerilogRequestLogging();

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
    app.UseResponseCaching();

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
    Log.Information(" Host built successfully.");


    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.Information("Shutting down the application");
    Log.CloseAndFlush();
}



//IDisposable 