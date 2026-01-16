//using api_gualan.Helpers;
//using api_gualan.Services;
//using Microsoft.AspNetCore.Http.Features;

//var builder = WebApplication.CreateBuilder(args);

//// 🔹 Servicios propios
//builder.Services.AddScoped<MySqlHelper>();
//builder.Services.AddScoped<CsvBatchService>();

//// 🔹 Controllers
//builder.Services.AddControllers();

//// 🔹 Límite CSV (500 MB)
//builder.Services.Configure<FormOptions>(options =>
//{
//    options.MultipartBodyLengthLimit = 524_288_000;
//});

//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.Limits.MaxRequestBodySize = 524_288_000;
//});

//// 🔹 Swagger
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// 🔹 PathBase IIS
//app.UsePathBase("/api_gualan");

//// 🔹 Redirect Swagger
//app.Use(async (context, next) =>
//{
//    if (context.Request.Path == "/" || context.Request.Path == "")
//    {
//        context.Response.Redirect("/api_gualan/swagger/index.html");
//        return;
//    }
//    await next();
//});

//// 🔹 Swagger
//app.UseSwagger();
//app.UseSwaggerUI(c =>
//{
//    c.SwaggerEndpoint("v1/swagger.json", "API GUALAN v1");
//    c.RoutePrefix = "swagger";
//});

//// 🔹 Middleware
//app.UseHttpsRedirection();
//app.UseAuthorization();

//app.MapControllers();
//app.Run();


//========nuevo program
using api_gualan.Helpers;
using api_gualan.Helpers.Interfaces;
using api_gualan.Helpers.MySql;
using api_gualan.Helpers.SqlServer;
using api_gualan.Services;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// 🔹 CONFIGURACIÓN CSV
// =====================================================
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 524_288_000; // 500 MB
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 524_288_000;
});

// =====================================================
// 🔹 REGISTRO DE HELPERS DE BASE DE DATOS
// =====================================================
builder.Services.AddScoped<MySqlServerHelper>();
builder.Services.AddScoped<SqlServerHelper>();

builder.Services.AddScoped<IDbHelper>(sp =>
{
    try
    {
        return DbHelperFactory.Create(sp); // Lazy connection: no valida DB hasta usar
    }
    catch (Exception ex)
    {
        var logger = sp.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creando DbHelper. La app seguirá levantando.");
        return null; // DB falla, la app sigue
    }
});

// =====================================================
// 🔹 SERVICIOS
// =====================================================
builder.Services.AddScoped<CsvBatchService>();

// =====================================================
// 🔹 CONTROLLERS
// =====================================================
builder.Services.AddControllers();

// =====================================================
// 🔹 SWAGGER
// =====================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// =====================================================
// 🔹 CREAR CARPETA LOGS SI NO EXISTE
// =====================================================
var logPath = @"C:\Logs";
if (!Directory.Exists(logPath))
{
    Directory.CreateDirectory(logPath);
}

// =====================================================
// 🔹 PATH BASE IIS
// =====================================================
app.UsePathBase("/api_gualan");

// Redirect raíz → Swagger seguro
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/" || string.IsNullOrEmpty(context.Request.Path))
    {
        context.Response.Redirect("/api_gualan/swagger/index.html");
        return;
    }
    await next();
});

// =====================================================
// 🔹 MIDDLEWARE
// =====================================================
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("v1/swagger.json", "API GUALAN v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// =====================================================
// 🔹 CAPTURA ERRORES GLOBALES
// =====================================================
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logFile = Path.Combine(logPath, $"Error_{DateTime.Now:yyyyMMdd}.log");
        await File.AppendAllTextAsync(logFile, $"[{DateTime.Now:HH:mm:ss}] {ex}\n\n");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Error interno del servidor. Revise logs.");
    }
});

app.Run();
