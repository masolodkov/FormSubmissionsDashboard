using FormService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Form Submission API",
        Version = "v1",
        Description = "A form submission system, agnostic of the form structure, with flexible storage and search capabilities",
        Contact = new Microsoft.OpenApi.OpenApiContact
        {
            Name = "Mikhail Solodkov",
            Email = "masolodkov@gmail.com"
        }
    });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

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

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Form Submission API v1");
});

app.MapControllers();

app.Run();
