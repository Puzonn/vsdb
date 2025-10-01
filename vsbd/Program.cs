using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "cors",
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:1420");
                          policy.AllowAnyHeader();
                          policy.AllowAnyMethod();
                      });
});

builder.Services.AddScoped<BuildService>();

var app = builder.Build();

app.UseCors("cors");
app.MapControllers();


app.Run();

record RoslynCompileResponse(bool Success, string? DllPath, string? PdbPath, string[] Diagnostics);
