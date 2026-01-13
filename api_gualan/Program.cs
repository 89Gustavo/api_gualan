using api_gualan.Helpers;
using api_gualan.Services;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Servicios propios
builder.Services.AddScoped<MySqlHelper>();
builder.Services.AddScoped<CsvBatchService>();

// 🔹 Controllers
builder.Services.AddControllers();

// 🔹 Límite CSV (500 MB)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 524_288_000;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 524_288_000;
});

// 🔹 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 🔹 PathBase IIS
app.UsePathBase("/api_gualan");

// 🔹 Redirect Swagger
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/" || context.Request.Path == "")
    {
        context.Response.Redirect("/api_gualan/swagger/index.html");
        return;
    }
    await next();
});

// 🔹 Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("v1/swagger.json", "API GUALAN v1");
    c.RoutePrefix = "swagger";
});

// 🔹 Middleware
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.Run();
