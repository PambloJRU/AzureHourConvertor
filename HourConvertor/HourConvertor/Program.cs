var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// 1. Agregar el soporte para Controladores
builder.Services.AddControllers();

// 2. Configurar CORS (igual que antes)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// 3. Registrar el Factory de HttpClient (Buena práctica para Controladores)
builder.Services.AddHttpClient();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.UseDefaultFiles(); // Esto hace que busque automáticamente un "index.html"
app.UseStaticFiles();  // Esto permite que el servidor web entregue el archivo
app.Run();
