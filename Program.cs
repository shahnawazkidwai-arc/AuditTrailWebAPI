using audit_trail_.NET_Core_Web_API.Repositories;
using audit_trail_.NET_Core_Web_API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(opts =>
{
    // preserve property names-case
    opts.JsonSerializerOptions.PropertyNamingPolicy = null;
});

// DI
builder.Services.AddSingleton<IAuditRepository, InMemoryAuditRepository>();
builder.Services.AddScoped<IAuditService, AuditService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
