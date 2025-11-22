using FormService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IDataStorage, InMemoryStorage>();
builder.Services.AddScoped<IFormProcessor, DocumentFormProcessor>();
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
