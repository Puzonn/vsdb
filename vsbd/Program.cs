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
                          policy.AllowCredentials();
                      });
});

builder.Services.AddLogging();
builder.Services.AddScoped<BuildService>();
builder.Services.AddScoped<FlowRunnerService>();
builder.Services.AddSingleton<IFlowJobManager, FlowJobManager>();

builder.Services.AddSignalR();

var app = builder.Build();

app.UseCors("cors");

app.MapHub<FlowHub>("/flow");

app.MapControllers();


app.Run();

record RoslynCompileResponse(bool Success, string? DllPath, string? PdbPath, string[] Diagnostics);
