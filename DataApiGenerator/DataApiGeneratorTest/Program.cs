using DigitalSolutions.Data;
using DigitalSolutions.Data.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DemoContext>(options =>
{
  options.UseSqlite("Data Source=testdb.s3db");
});

builder.Services.AddTransient<PersonService>();
builder.Services.AddTransient<JobDefinitionService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();
