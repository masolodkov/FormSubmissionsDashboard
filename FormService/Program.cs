using FormService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IDataStorage, InMemoryStorage>();
builder.Services.AddScoped<IFormProcessor, DocumentFormProcessor>();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();
app.UseCors("AllowAll");

app.MapControllers();

app.Run();
