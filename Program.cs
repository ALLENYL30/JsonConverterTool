using JsonConverterTool.Services;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JSON Converter API",
        Version = "v1",
        Description = "An API for converting JSON to C# and Java code",
        Contact = new OpenApiContact
        {
            Name = "JSON Converter Tool",
            Email = "support@jsonconverter.example.com"
        }
    });

    // Set the comments path for the Swagger JSON and UI
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // Use fully qualified object names to avoid conflicts
    c.CustomSchemaIds(type => type.FullName);
});

// Register our services
builder.Services.AddScoped<IJsonConverterService, JsonConverterService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// IMPORTANT: UseCors must be called before UseRouting and UseEndpoints
app.UseCors("AllowReactApp");

if (app.Environment.IsDevelopment())
{
    // Enable Swagger UI in development
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "JSON Converter API v1");
        c.RoutePrefix = "swagger";
    });
}

// Comment out HTTPS redirection for testing with HTTP
// app.UseHttpsRedirection();

app.MapControllers();

app.Run();
