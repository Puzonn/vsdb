using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "cors",
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5173");
                          policy.AllowAnyHeader();
                          policy.AllowAnyMethod();
                          policy.AllowCredentials();
                      });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
);

builder.Services.AddLogging();
builder.Services.AddSingleton<PathService>();
builder.Services.AddScoped<SharedLibraryService>();
builder.Services.AddScoped<BuildService>();
builder.Services.AddScoped<BuildService>();
builder.Services.AddScoped<ProjectRepository>();
builder.Services.AddScoped<IFlowJobManager, FlowJobManager>();
builder.Services.AddScoped<NodeFactory>();

builder.Services.AddSignalR();


var app = builder.Build();

app.UseCors("cors");

app.MapHub<FlowHub>("/flow");

app.MapControllers();
app.UseStaticFiles();


app.Run();

record RoslynCompileResponse(bool Success, string? DllPath, string? PdbPath, string[] Diagnostics);
